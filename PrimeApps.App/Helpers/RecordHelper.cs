using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.Notifications;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Record;
using Hangfire;
using Microsoft.AspNetCore.Http;
using PrimeApps.App.Services;
using PrimeApps.Model.Entities.Platform;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace PrimeApps.App.Helpers
{
    public interface IRecordHelper
    {
        Task<int> BeforeCreateUpdate(Module module, JObject record, ModelStateDictionary modelState,
            string tenantLanguage, bool convertPicklists = true, JObject currentRecord = null, UserItem appUser = null);
        Task<int> BeforeDelete(Module module, JObject record, UserItem appUser);
        void AfterCreate(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true,
            bool runCalculations = true, int timeZoneOffset = 180, bool runDefaults = true);
        void AfterUpdate(Module module, JObject record, JObject currentRecord, UserItem appUser, Warehouse warehouse,
            bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180);
        void AfterDelete(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true,
            bool runCalculations = true);
        JObject PrepareConflictError(PostgresException ex);
        bool ValidateFilterLogic(string filterLogic, List<Filter> filters);
        Task CreateStageHistory(JObject record, JObject currentRecord = null);
        Task UpdateStageHistory(JObject record, JObject currentRecord);
        void SetCombinations(JObject record, JObject currentRecord, Field fieldCombination);
        Task SetActivityType(JObject record);
        Task SetTransactionType(JObject record);
        Task SetForecastFields(JObject record);
        JObject CreateStageHistoryRecord(JObject record, JObject currentRecord);
        string GetRecordPrimaryValue(JObject record, Module module);
        Task<List<string>> GetAllFieldsForFindRequest(string moduleName, bool withLookups = true);
        void SetCurrentUser(UserItem appUser);
    }

    public delegate Task<int> BeforeCreateUpdate(Module module, JObject record, ModelStateDictionary modelState,
        string tenantLanguage, bool convertPicklists = true, JObject currentRecord = null, UserItem appUser = null);

    public delegate Task UpdateStageHistory(JObject record, JObject currentRecord);

    public delegate void AfterUpdate(Module module, JObject record, JObject currentRecord, UserItem appUser,
        Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180);

    public delegate void AfterCreate(Module module, JObject record, UserItem appUser, Warehouse warehouse,
        bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180, bool runDefaults = true);

    public delegate Task<List<string>> GetAllFieldsForFindRequest(string moduleName, bool withLookups = true);

    public delegate bool ValidateFilterLogic(string filterLogic, List<Filter> filters);

    public class RecordHelper : IRecordHelper
    {

        private CurrentUser _currentUser;
        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;
        private IAuditLogHelper _auditLogHelper;
        private INotificationHelper _notificationHelper;
        private IWorkflowHelper _workflowHelper;
        private IProcessHelper _processHelper;
        private ICalculationHelper _calculationHelper;
        private IChangeLogHelper _changeLogHelper;
        private IHttpContextAccessor _context;
        public IBackgroundTaskQueue Queue { get; }

        public RecordHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IAuditLogHelper auditLogHelper, INotificationHelper notificationHelper, IWorkflowHelper workflowHelper, IProcessHelper processHelper, ICalculationHelper calculationHelper, IChangeLogHelper changeLogHelper, IBackgroundTaskQueue queue, IHttpContextAccessor context)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;

            _currentUser = UserHelper.GetCurrentUser(_context);

            _auditLogHelper = auditLogHelper;
            _notificationHelper = notificationHelper;
            _workflowHelper = workflowHelper;
            _processHelper = processHelper;
            _calculationHelper = calculationHelper;
            _changeLogHelper = changeLogHelper;

            Queue = queue;
        }
        public async Task<int> BeforeCreateUpdate(Module module, JObject record, ModelStateDictionary modelState, string tenantLanguage, bool convertPicklists = true, JObject currentRecord = null, UserItem appUser = null)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _picklistRepository.CurrentUser = _currentUser;
                    //TODO: Validate metadata
                    //TODO: Profile permission check
                    var picklistItemIds = new List<int>();
                    JObject recordNew = new JObject();
                    
                    // Check all fields is valid and prepare for related data types
                    foreach (var prop in record)
                    {
                        if (Model.Helpers.ModuleHelper.SystemFields.Contains(prop.Key))
                            continue;

                        if (Model.Helpers.ModuleHelper.ModuleSpecificFields(module).Contains(prop.Key))
                            continue;

                        var field = module.Fields.FirstOrDefault(x => x.Name == prop.Key);

                        if (field == null)
                        {
                            modelState.AddModelError(prop.Key, $"Field '{prop.Key}' not found.");
                            return -1;
                        }

                        if (prop.Value.IsNullOrEmpty())
                            continue;
                        

                        if (field.Encrypted)
                        {
                            recordNew[prop.Key] = null;
                            recordNew[prop.Key + "__encrypted"] = EncryptionHelper.Encrypt((string)prop.Value, field.Id, appUser, _configuration);
                        }

                        if (field.DataType == DataType.Picklist && convertPicklists)
                        {
                            if (!prop.Value.IsNumeric())
                            {
                                modelState.AddModelError(prop.Key, $"Value of '{prop.Key}' field must be numeric.");
                                return -1;
                            }

                            picklistItemIds.Add((int)prop.Value);
                        }
                        if (field.DataType == DataType.Tag)
                        {
                            var tagLabel = new List<string>();

                            string str = (string)prop.Value;

                            string[] tags = str.Split(',');


                            using (var _tagRepository = new TagRepository(databaseContext, _configuration))
                            {
                                _tagRepository.CurrentUser = _currentUser;
                                var fieldTags = await _tagRepository.GetByFieldId(field.Id);

                                foreach (string tag in tags)
                                {
                                    tagLabel.Add(tag);
                                    var fieldTag = fieldTags.Where(x => x.Text == tag).ToList();

                                    if (fieldTag.Count <= 0)
                                    {
                                        var creatTag = new Tag
                                        {
                                            Text = tag,
                                            FieldId = field.Id
                                        };

                                        await _tagRepository.Create(creatTag);
                                    }
                                }
                            }


                            record[prop.Key] = string.Join("|", tagLabel);
                        }
                        if (field.DataType == DataType.Multiselect && convertPicklists)
                        {
                            if (!(prop.Value is JArray))
                            {
                                modelState.AddModelError(prop.Key, $"Value of '{prop.Key}' field must be array.");
                                return -1;
                            }

                            var multiselectPicklistItemIds = new List<int>();

                            foreach (var item in (JArray)prop.Value)
                            {
                                if (!item.IsNumeric())
                                {
                                    modelState.AddModelError(prop.Key, $"All items of '{prop.Key}' field value must be numeric.");
                                    return -1;
                                }

                                multiselectPicklistItemIds.Add((int)item);
                            }

                            picklistItemIds.AddRange(multiselectPicklistItemIds);
                        }
                    }

                    if (!recordNew.IsNullOrEmpty())
                    {
                        foreach (var prop in recordNew)
                        {
                            record[prop.Key] = recordNew[prop.Key];
                        }
                    }

                    //Module specific actions
                    switch (module.Name)
                    {
                        case "activities":
                            if (!record["activity_type"].IsNullOrEmpty())
                                await SetActivityType(record);
                            break;
                        case "opportunities":
                            await SetForecastFields(record);
                            break;
                        case "current_accounts":
                            await SetTransactionType(record);
                            break;
                    }

                    // Check picklists and set picklist's label to record value
                    if (picklistItemIds.Count > 0 && convertPicklists)
                    {
                        var picklistItems = await _picklistRepository.FindItems(picklistItemIds);
                        var hasModulePicklists = picklistItemIds.Any(x => x > 900000);

                        if ((picklistItems == null || picklistItems.Count < 1) && !hasModulePicklists)
                        {
                            modelState.AddModelError("picklist", "Picklists not found.");
                            return -1;
                        }

                        foreach (var pair in record)
                        {
                            if (Model.Helpers.ModuleHelper.SystemFields.Contains(pair.Key))
                                continue;

                            var field = module.Fields.FirstOrDefault(x => x.Name == pair.Key);

                            if (field == null)
                                continue;

                            if (field.DataType == DataType.Picklist)
                            {
                                if (pair.Value.IsNullOrEmpty())
                                    continue;

                                if (field.Name != "related_module")
                                {
                                    var picklistItem = picklistItems.FirstOrDefault(x => x.Id == (int)pair.Value && x.PicklistId == field.PicklistId.Value);

                                    if (picklistItem == null)
                                    {
                                        modelState.AddModelError(pair.Key, $"Picklist item '{pair.Value}' not found.");
                                        return -1;
                                    }

                                    record[pair.Key] = tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn;
                                }
                                else
                                {
                                    var relatedModule = await _moduleRepository.GetByIdBasic((int)pair.Value - 900000);

                                    if (relatedModule == null)
                                    {
                                        modelState.AddModelError(pair.Key, $"Picklist item '{pair.Value}' not found.");
                                        return -1;
                                    }

                                    record[pair.Key] = tenantLanguage == "tr" ? relatedModule.LabelTrSingular : relatedModule.LabelEnSingular;
                                }
                            }

                            if (field.DataType == DataType.Multiselect)
                            {
                                if (pair.Value.IsNullOrEmpty())
                                    continue;

                                var picklistLabels = new List<string>();

                                foreach (var item in (JArray)pair.Value)
                                {
                                    var picklistItem = picklistItems.FirstOrDefault(x => x.Id == (int)item && x.PicklistId == field.PicklistId.Value);

                                    if (picklistItem == null)
                                    {
                                        modelState.AddModelError(pair.Key, $"Picklist item '{pair.Value}' not found.");
                                        return -1;
                                    }

                                    picklistLabels.Add(tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn);
                                }

                                record[pair.Key] = string.Join("|", picklistLabels);
                            }
                        }
                    }

                    // Process combination
                    var fieldCombinations = module.Fields.Where(x => x.Combination != null);

                    foreach (var fieldCombination in fieldCombinations)
                    {
                        SetCombinations(record, currentRecord, fieldCombination);
                    }

                    // Check freeze
                    if (!currentRecord.IsNullOrEmpty() && !currentRecord["process_id"].IsNullOrEmpty())
                    {
                        if ((int)currentRecord["process_status"] != 3 && (int)currentRecord["operation_type"] != 1 && appUser != null && !appUser.HasAdminProfile)
                            record["freeze"] = true;
                        else
                            record["freeze"] = false;
                    }
                    return 0;
                }
            }
        }

        public async Task<int> BeforeDelete(Module module, JObject record, UserItem appUser)
        {
            //TODO: Profile permission check

            // Check freeze
            if (!record.IsNullOrEmpty() && !record["process_id"].IsNullOrEmpty())
            {

                if (appUser != null && !appUser.HasAdminProfile)
                    record["freeze"] = true;
                else
                    record["freeze"] = false;
            }
            return 0;
        }

        public void AfterCreate(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180, bool runDefaults = true)
        {
            if (runDefaults)
            {
                Queue.QueueBackgroundWorkItem(async token => await _auditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(record, module), AuditType.Record, RecordActionType.Inserted, null, module));
                Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.Create(appUser, record, module, warehouse, timeZoneOffset));
                Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.SendTaskNotification(record, appUser, module));
            }

            if (runWorkflows)
            {
                Queue.QueueBackgroundWorkItem(async token => await _workflowHelper.Run(OperationType.insert, record, module, appUser, warehouse, BeforeCreateUpdate, UpdateStageHistory, AfterUpdate, AfterCreate));
                Queue.QueueBackgroundWorkItem(async token => await _processHelper.Run(OperationType.insert, record, module, appUser, warehouse, ProcessTriggerTime.Instant, BeforeCreateUpdate, GetAllFieldsForFindRequest, UpdateStageHistory, AfterUpdate, AfterCreate));
            }


            if (runCalculations)
            {
                Queue.QueueBackgroundWorkItem(async token => await _calculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.insert, BeforeCreateUpdate, GetAllFieldsForFindRequest));
            }
        }

        public void AfterUpdate(Module module, JObject record, JObject currentRecord, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180)
        {
            Queue.QueueBackgroundWorkItem(async token => await _changeLogHelper.CreateLog(appUser, currentRecord, module));
            Queue.QueueBackgroundWorkItem(async token => await _auditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(currentRecord, module), AuditType.Record, RecordActionType.Updated, null, module));
            Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.Update(appUser, record, currentRecord, module, warehouse, timeZoneOffset));
            Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.SendTaskNotification(record, appUser, module));

            if (runWorkflows)
            {
                Queue.QueueBackgroundWorkItem(async token => await _workflowHelper.Run(OperationType.update, record, module, appUser, warehouse, BeforeCreateUpdate, UpdateStageHistory, AfterUpdate, AfterCreate));
                Queue.QueueBackgroundWorkItem(async token => await _processHelper.Run(OperationType.update, record, module, appUser, warehouse, ProcessTriggerTime.Instant, BeforeCreateUpdate, GetAllFieldsForFindRequest, UpdateStageHistory, AfterUpdate, AfterCreate));

                //if (currentRecord["process_id"].IsNullOrEmpty())
                //{
                //    HostingEnvironment.QueueBackgroundWorkItem(clt => ProcessHelper.Run(OperationType.update, record, module, appUser, warehouse));
                //}
                //else if (!currentRecord["process_status"].IsNullOrEmpty() && (int)currentRecord["process_status"] == 2)
                //{
                //    HostingEnvironment.QueueBackgroundWorkItem(clt => ProcessHelper.SendToApprovalApprovedRequest(OperationType.update, currentRecord, appUser, warehouse));
                //}
            }


            if (runCalculations)
            {
                Queue.QueueBackgroundWorkItem(async token => _calculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.update, BeforeCreateUpdate, GetAllFieldsForFindRequest));
            }
        }

        public void AfterDelete(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true)
        {
            Queue.QueueBackgroundWorkItem(async token => await _auditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(record, module), AuditType.Record, RecordActionType.Deleted, null, module));
            Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.Delete(appUser, record, module));

            if (runWorkflows)
            {
                Queue.QueueBackgroundWorkItem(async token => await _workflowHelper.Run(OperationType.delete, record, module, appUser, warehouse, BeforeCreateUpdate, UpdateStageHistory, AfterUpdate, AfterCreate));
                Queue.QueueBackgroundWorkItem(async token => await _processHelper.Run(OperationType.delete, record, module, appUser, warehouse, ProcessTriggerTime.Instant, BeforeCreateUpdate, GetAllFieldsForFindRequest, UpdateStageHistory, AfterUpdate, AfterCreate));
            }


            if (runCalculations)
            {
                Queue.QueueBackgroundWorkItem(async token => await _calculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.delete, BeforeCreateUpdate, GetAllFieldsForFindRequest));
            }
        }

        public JObject PrepareConflictError(PostgresException ex)
        {
            var field = ex.ConstraintName;
            var field2 = string.Empty;

            var moduleName = ex.TableName.TrimEnd('d').TrimEnd('_');

            if (field.StartsWith(moduleName + "_"))
                field = field.Remove(0, moduleName.Length + 1);

            if (field.StartsWith("ix_unique_"))
                field = field.Remove(0, "ix_unique_".Length);

            if (field.Contains("-"))
            {
                var fieldParts = field.Split('-');
                field = fieldParts[0];
                field2 = fieldParts[1];
            }

            var error = new JObject();
            error["field"] = field;
            error["message"] = ex.Detail;

            if (!string.IsNullOrEmpty(field2))
                error["field2"] = field2;


            return error;
        }

        public bool ValidateFilterLogic(string filterLogic, List<Filter> filters)
        {
            if (string.IsNullOrWhiteSpace(filterLogic))
                return true;

            if (filters == null || filters.Count < 1)
                return false;

            var digits = filterLogic.Where(char.IsDigit).ToList();

            foreach (var digit in digits)
            {
                var filterNo = byte.Parse(digit.ToString());
                var filter = filters.FirstOrDefault(x => x.No == filterNo);

                if (filter == null)
                    return false;
            }

            return true;
        }

        public async Task CreateStageHistory(JObject record, JObject currentRecord = null)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;
                    if (record["stage"] != null)
                    {
                        var stageHistoryModule = await _moduleRepository.GetByNameBasic("stage_history");
                        var stageHistory = CreateStageHistoryRecord(record, currentRecord);

                        await _recordRepository.Create(stageHistory, stageHistoryModule);
                    }
                }
            }

        }

        public async Task UpdateStageHistory(JObject record, JObject currentRecord)
        {
            if (record["stage"] != null)
            {
                if ((string)record["stage"] != (string)currentRecord["stage"])
                    await CreateStageHistory(record, currentRecord);
            }
        }

        public void SetCombinations(JObject record, JObject currentRecord, Field fieldCombination)
        {
            var isSet = false;
            var combinationCharacter = !string.IsNullOrWhiteSpace(fieldCombination.Combination.CombinationCharacter) ? fieldCombination.Combination.CombinationCharacter : " ";

            if (combinationCharacter == "<+>")
                combinationCharacter = "";

            if (!record[fieldCombination.Combination.Field1].IsNullOrEmpty() && !record[fieldCombination.Combination.Field2].IsNullOrEmpty())
            {
                record[fieldCombination.Name] = record[fieldCombination.Combination.Field1] + combinationCharacter + record[fieldCombination.Combination.Field2];
                isSet = true;
            }

            if (!isSet && currentRecord != null)
            {
                if (!record[fieldCombination.Combination.Field1].IsNullOrEmpty() && record[fieldCombination.Combination.Field2].IsNullOrEmpty() && !currentRecord[fieldCombination.Combination.Field2].IsNullOrEmpty())
                {
                    record[fieldCombination.Name] = record[fieldCombination.Combination.Field1] + combinationCharacter + currentRecord[fieldCombination.Combination.Field2];
                    isSet = true;
                }

                if (record[fieldCombination.Combination.Field1].IsNullOrEmpty() && !record[fieldCombination.Combination.Field2].IsNullOrEmpty() && !currentRecord[fieldCombination.Combination.Field1].IsNullOrEmpty())
                {
                    record[fieldCombination.Name] = currentRecord[fieldCombination.Combination.Field1] + combinationCharacter + record[fieldCombination.Combination.Field2];
                    isSet = true;
                }
            }

            if (!isSet && !record[fieldCombination.Combination.Field1].IsNullOrEmpty())
                record[fieldCombination.Name] = record[fieldCombination.Combination.Field1];

            if (!isSet && !record[fieldCombination.Combination.Field2].IsNullOrEmpty())
                record[fieldCombination.Name] = record[fieldCombination.Combination.Field2];
        }

        public async Task SetActivityType(JObject record)
        {
            if (!record["id"].IsNullOrEmpty() && record["activity_type"].IsNullOrEmpty())
                return;

            if (record["id"].IsNullOrEmpty() && record["activity_type"].IsNullOrEmpty())
                throw new Exception("ActivityType not found!");

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                {
                    _picklistRepository.CurrentUser = _currentUser;
                    var activityType = await _picklistRepository.GetItemById((int)record["activity_type"]);

                    if (activityType == null)
                        throw new Exception("ActivityType picklist item not found!");

                    record["activity_type_system"] = activityType.Value;
                }
            }
        }

        public async Task SetTransactionType(JObject record)
        {
            if (!record["id"].IsNullOrEmpty() && record["transaction_type"].IsNullOrEmpty())
                return;

            if (record["id"].IsNullOrEmpty() && record["transaction_type"].IsNullOrEmpty())
                throw new Exception("TransactionType not found!");

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                {
                    _picklistRepository.CurrentUser = _currentUser;
                    var transactionType = await _picklistRepository.GetItemById((int)record["transaction_type"]);

                    if (transactionType == null)
                        throw new Exception("TransactionType picklist item not found!");

                    record["transaction_type_system"] = transactionType.Value;
                }
            }
        }

        public async Task SetForecastFields(JObject record)
        {
            if (!record["stage"].IsNullOrEmpty())
            {
                using (var _scope = _serviceScopeFactory.CreateScope())
                {
                    var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                    using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                    {
                        _picklistRepository.CurrentUser = _currentUser;
                        var stage = await _picklistRepository.GetItemById((int)record["stage"]);

                        if (stage == null)
                            throw new Exception("Stage picklist not found!");

                        record["forecast_type"] = stage.Value2;
                        record["forecast_category"] = stage.Value3;
                    }
                }
            }

            if (!record["closing_date"].IsNullOrEmpty())
            {
                var closingDate = (DateTime)record["closing_date"];

                record["forecast_year"] = closingDate.Year;
                record["forecast_month"] = closingDate.Month;
                record["forecast_quarter"] = closingDate.ToQuarter();
            }
        }

        public JObject CreateStageHistoryRecord(JObject record, JObject currentRecord)
        {
            var stageHistory = new JObject();
            stageHistory["opportunity"] = record["id"];
            stageHistory["stage"] = record["stage"];

            if (currentRecord != null)
            {
                stageHistory["amount"] = !record["amount"].IsNullOrEmpty() ? record["amount"] : currentRecord["amount"];
                stageHistory["closing_date"] = !record["closing_date"].IsNullOrEmpty() ? record["closing_date"] : currentRecord["closing_date"];
                stageHistory["probability"] = !record["probability"].IsNullOrEmpty() ? record["probability"] : currentRecord["probability"];
                stageHistory["expected_revenue"] = !record["expected_revenue"].IsNullOrEmpty() ? record["expected_revenue"] : currentRecord["expected_revenue"];
            }
            else
            {
                stageHistory["amount"] = !record["amount"].IsNullOrEmpty() ? record["amount"] : null;
                stageHistory["closing_date"] = !record["closing_date"].IsNullOrEmpty() ? record["closing_date"] : null;
                stageHistory["probability"] = !record["probability"].IsNullOrEmpty() ? record["probability"] : null;
                stageHistory["expected_revenue"] = !record["expected_revenue"].IsNullOrEmpty() ? record["expected_revenue"] : null;
            }

            return stageHistory;
        }

        public string GetRecordPrimaryValue(JObject record, Module module)
        {
            var recordName = string.Empty;
            var primaryField = module.Fields.FirstOrDefault(x => x.Primary);

            if (primaryField != null)
                recordName = (string)record[primaryField.Name];

            return recordName;
        }

        public async Task<List<string>> GetAllFieldsForFindRequest(string moduleName, bool withLookups = true)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _currentUser;
                    var module = await _moduleRepository.GetByNameBasic(moduleName);
                    var fields = new List<string>();

                    foreach (var field in module.Fields)
                    {
                        if (field.Deleted || field.LookupType == "relation")
                            continue;

                        if (field.DataType == DataType.Lookup && withLookups)
                        {
                            Module lookupModule;

                            if (field.LookupType == "users")
                                lookupModule = Model.Helpers.ModuleHelper.GetFakeUserModule();
                            else
                                lookupModule = await _moduleRepository.GetByName(field.LookupType);

                            foreach (var lookupModuleField in lookupModule.Fields)
                            {
                                if (lookupModuleField.Deleted || lookupModuleField.DataType == DataType.Lookup)
                                    continue;

                                fields.Add(field.Name + "." + field.LookupType + "." + lookupModuleField.Name);
                            }
                        }
                        else
                        {
                            fields.Add(field.Name);
                        }
                    }

                    return fields;
                }
            }

        }

        /// <summary>
        /// Sets current user in case if it's unavailable from context.
        /// </summary>
        /// <param name="currentUser"></param>
        public void SetCurrentUser(UserItem appUser)
        {
            _currentUser = new CurrentUser()
            {
                TenantId = appUser.TenantId,
                UserId = appUser.Id

            };
        }
    }
}