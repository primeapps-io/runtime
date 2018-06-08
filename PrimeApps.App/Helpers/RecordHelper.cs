using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Internal;
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

/*
 * using OfisimCRM.DTO.AuditLog;
using OfisimCRM.DTO.Cache;
using OfisimCRM.DTO.Record;
using OfisimCRM.Model.Entities;
using OfisimCRM.Model.Enums;
using OfisimCRM.Model.Helpers;
using OfisimCRM.Model.Repositories.Interfaces;
using OfisimCRM.App.Notifications;
using OfisimCRM.DTO.Common;
 */

namespace PrimeApps.App.Helpers
{
    public static class RecordHelper
    {
        public static async Task<int> BeforeCreateUpdate(Module module, JObject record, ModelStateDictionary modelState, string tenantLanguage, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, bool convertPicklists = true, JObject currentRecord = null, UserItem appUser = null)
        {
            //TODO: Validate metadata
            //TODO: Profile permission check
            var picklistItemIds = new List<int>();

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

                if (field.DataType == DataType.Picklist && convertPicklists)
                {
                    if (!prop.Value.IsNumeric())
                    {
                        modelState.AddModelError(prop.Key, $"Value of '{prop.Key}' field must be numeric.");
                        return -1;
                    }

                    picklistItemIds.Add((int)prop.Value);
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

            //Module specific actions
            switch (module.Name)
            {
                case "activities":
                    if (!record["activity_type"].IsNullOrEmpty())
                        await SetActivityType(record, picklistRepository);
                    break;
                case "opportunities":
                    await SetForecastFields(record, picklistRepository);
                    break;
                case "current_accounts":
                    await SetTransactionType(record, picklistRepository);
                    break;
            }

            // Check picklists and set picklist's label to record value
            if (picklistItemIds.Count > 0 && convertPicklists)
            {
                var picklistItems = await picklistRepository.FindItems(picklistItemIds);
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
                            var relatedModule = await moduleRepository.GetByIdBasic((int)pair.Value - 900000);

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

        public static async Task<int> BeforeDelete(Module module, JObject record, UserItem appUser)
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

        public static void AfterCreate(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180, bool runDefaults = true)
        {
            if (runDefaults)
            {
				BackgroundJob.Enqueue(() => AuditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(record, module), AuditType.Record, RecordActionType.Inserted, null, module));
				BackgroundJob.Enqueue(() => NotificationHelper.Create(appUser, record, module, timeZoneOffset));
				BackgroundJob.Enqueue(() => NotificationHelper.SendTaskNotification(record, appUser, module));

				//HostingEnvironment.QueueBackgroundWorkItem(clt => AuditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(record, module), AuditType.Record, RecordActionType.Inserted, null, module));
                //HostingEnvironment.QueueBackgroundWorkItem(clt => NotificationHelper.Create(appUser, record, module, timeZoneOffset));
                //HostingEnvironment.QueueBackgroundWorkItem(clt => NotificationHelper.SendTaskNotification(record, appUser, module));
            }

            if (runWorkflows)
            {
				BackgroundJob.Enqueue(() => WorkflowHelper.Run(OperationType.insert, record, module, appUser, warehouse));
				BackgroundJob.Enqueue(() => ProcessHelper.Run(OperationType.insert, record, module, appUser, warehouse, ProcessTriggerTime.Instant));

				//HostingEnvironment.QueueBackgroundWorkItem(clt => WorkflowHelper.Run(OperationType.insert, record, module, appUser, warehouse));
                //HostingEnvironment.QueueBackgroundWorkItem(clt => ProcessHelper.Run(OperationType.insert, record, module, appUser, warehouse, ProcessTriggerTime.Instant));
            }


			if (runCalculations)
			{
				BackgroundJob.Enqueue(() => CalculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.insert));
				//HostingEnvironment.QueueBackgroundWorkItem(clt => CalculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.insert));
			}
        }

        public static void AfterUpdate(Module module, JObject record, JObject currentRecord, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180)
        {
			BackgroundJob.Enqueue(() => ChangeLogHelper.CreateLog(appUser, currentRecord, module));
			BackgroundJob.Enqueue(() => AuditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(currentRecord, module), AuditType.Record, RecordActionType.Updated, null, module));
			BackgroundJob.Enqueue(() => NotificationHelper.Update(appUser, record, currentRecord, module, timeZoneOffset));
			BackgroundJob.Enqueue(() => NotificationHelper.SendTaskNotification(record, appUser, module));

			//HostingEnvironment.QueueBackgroundWorkItem(clt => ChangeLogHelper.CreateLog(appUser, currentRecord, module));
            //HostingEnvironment.QueueBackgroundWorkItem(clt => AuditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(currentRecord, module), AuditType.Record, RecordActionType.Updated, null, module));
            //HostingEnvironment.QueueBackgroundWorkItem(clt => NotificationHelper.Update(appUser, record, currentRecord, module, timeZoneOffset));
            //HostingEnvironment.QueueBackgroundWorkItem(clt => NotificationHelper.SendTaskNotification(record, appUser, module));

            if (runWorkflows)
            {
				BackgroundJob.Enqueue(() => WorkflowHelper.Run(OperationType.update, record, module, appUser, warehouse));
				BackgroundJob.Enqueue(() => ProcessHelper.Run(OperationType.update, record, module, appUser, warehouse, ProcessTriggerTime.Instant));

				//HostingEnvironment.QueueBackgroundWorkItem(clt => WorkflowHelper.Run(OperationType.update, record, module, appUser, warehouse));
                //HostingEnvironment.QueueBackgroundWorkItem(clt => ProcessHelper.Run(OperationType.update, record, module, appUser, warehouse, ProcessTriggerTime.Instant));

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
				BackgroundJob.Enqueue(() => CalculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.update));
				//HostingEnvironment.QueueBackgroundWorkItem(clt => CalculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.update));
			}
        }

        public static void AfterDelete(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true)
        {
			BackgroundJob.Enqueue(() => AuditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(record, module), AuditType.Record, RecordActionType.Deleted, null, module));
			BackgroundJob.Enqueue(() => NotificationHelper.Delete(appUser, record, module));

			//HostingEnvironment.QueueBackgroundWorkItem(clt => AuditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(record, module), AuditType.Record, RecordActionType.Deleted, null, module));
            //HostingEnvironment.QueueBackgroundWorkItem(clt => NotificationHelper.Delete(appUser, record, module));

            if (runWorkflows)
            {
				BackgroundJob.Enqueue(() => WorkflowHelper.Run(OperationType.delete, record, module, appUser, warehouse));
				BackgroundJob.Enqueue(() => ProcessHelper.Run(OperationType.delete, record, module, appUser, warehouse, ProcessTriggerTime.Instant));

				//HostingEnvironment.QueueBackgroundWorkItem(clt => WorkflowHelper.Run(OperationType.delete, record, module, appUser, warehouse));
                //HostingEnvironment.QueueBackgroundWorkItem(clt => ProcessHelper.Run(OperationType.delete, record, module, appUser, warehouse, ProcessTriggerTime.Instant));
            }


			if (runCalculations)
			{
				BackgroundJob.Enqueue(() => CalculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.delete));
				//HostingEnvironment.QueueBackgroundWorkItem(clt => CalculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.delete));
			}
        }

        public static JObject PrepareConflictError(PostgresException ex)
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

        public static bool ValidateFilterLogic(string filterLogic, List<Filter> filters)
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

        public static async Task CreateStageHistory(JObject record, IRecordRepository recordRepository, IModuleRepository moduleRepository, JObject currentRecord = null)
        {
            if (record["stage"] != null)
            {
                var stageHistoryModule = await moduleRepository.GetByNameBasic("stage_history");
                var stageHistory = CreateStageHistoryRecord(record, currentRecord);

                await recordRepository.Create(stageHistory, stageHistoryModule);
            }
        }

        public static async Task UpdateStageHistory(JObject record, JObject currentRecord, IRecordRepository recordRepository, IModuleRepository moduleRepository)
        {
            if (record["stage"] != null)
            {
                if ((string)record["stage"] != (string)currentRecord["stage"])
                    await CreateStageHistory(record, recordRepository, moduleRepository, currentRecord);
            }
        }

        public static void SetCombinations(JObject record, JObject currentRecord, Field fieldCombination)
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

        private static async Task SetActivityType(JObject record, IPicklistRepository picklistRepository)
        {
            if (!record["id"].IsNullOrEmpty() && record["activity_type"].IsNullOrEmpty())
                return;

            if (record["id"].IsNullOrEmpty() && record["activity_type"].IsNullOrEmpty())
                throw new Exception("ActivityType not found!");

            var activityType = await picklistRepository.GetItemById((int)record["activity_type"]);

            if (activityType == null)
                throw new Exception("ActivityType picklist item not found!");

            record["activity_type_system"] = activityType.Value;
        }

        private static async Task SetTransactionType(JObject record, IPicklistRepository picklistRepository)
        {
            if (!record["id"].IsNullOrEmpty() && record["transaction_type"].IsNullOrEmpty())
                return;

            if (record["id"].IsNullOrEmpty() && record["transaction_type"].IsNullOrEmpty())
                throw new Exception("TransactionType not found!");

            var transactionType = await picklistRepository.GetItemById((int)record["transaction_type"]);

            if (transactionType == null)
                throw new Exception("TransactionType picklist item not found!");

            record["transaction_type_system"] = transactionType.Value;
        }

        private static async Task SetForecastFields(JObject record, IPicklistRepository picklistRepository)
        {
            if (!record["stage"].IsNullOrEmpty())
            {
                var stage = await picklistRepository.GetItemById((int)record["stage"]);

                if (stage == null)
                    throw new Exception("Stage picklist not found!");

                record["forecast_type"] = stage.Value2;
                record["forecast_category"] = stage.Value3;
            }

            if (!record["closing_date"].IsNullOrEmpty())
            {
                var closingDate = (DateTime)record["closing_date"];

                record["forecast_year"] = closingDate.Year;
                record["forecast_month"] = closingDate.Month;
                record["forecast_quarter"] = closingDate.ToQuarter();
            }
        }

        private static JObject CreateStageHistoryRecord(JObject record, JObject currentRecord)
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

        private static string GetRecordPrimaryValue(JObject record, Module module)
        {
            var recordName = string.Empty;
            var primaryField = module.Fields.FirstOrDefault(x => x.Primary);

            if (primaryField != null)
                recordName = (string)record[primaryField.Name];

            return recordName;
        }

        public static async Task<List<string>> GetAllFieldsForFindRequest(string moduleName, IModuleRepository moduleRepository, bool withLookups = true)
        {
            var module = await moduleRepository.GetByNameBasic(moduleName);
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
                        lookupModule = await moduleRepository.GetByName(field.LookupType);

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