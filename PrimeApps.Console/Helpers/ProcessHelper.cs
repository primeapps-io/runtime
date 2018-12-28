using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Common.Resources;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Console.Models;

namespace PrimeApps.Console.Helpers
{
    public interface IProcessHelper
    {
        Task Run(OperationType operationType, JObject record, Module module, UserItem appUser, Warehouse warehouse, ProcessTriggerTime triggerTime, BeforeCreateUpdate BeforeCreateUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest, UpdateStageHistory UpdateStageHistory, AfterUpdate AfterUpdate, AfterCreate AfterCreate);
        Task<Process> CreateEntity(ProcessBindingModel processModel, string tenantLanguage, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, Warehouse warehouse, UserItem appUser);
        Task UpdateEntity(ProcessBindingModel processModel, Process process, string tenantLanguage);
        Task ApproveRequest(ProcessRequest request, UserItem appUser, Warehouse warehouse, BeforeCreateUpdate BeforeCreateUpdate, AfterUpdate AfterUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest);
        Task RejectRequest(ProcessRequest request, string message, UserItem appUser, Warehouse warehouse);
        Task SendToApprovalAgain(ProcessRequest request, UserItem appUser, Warehouse warehouse, BeforeCreateUpdate BeforeCreateUpdate, AfterUpdate AfterUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest);
        Task AfterCreateProcess(ProcessRequest request, UserItem appUser, Warehouse warehouse, BeforeCreateUpdate BeforeCreateUpdate, UpdateStageHistory UpdateStageHistory, AfterUpdate AfterUpdate, AfterCreate AfterCreate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest);
    }

    public class ProcessHelper : IProcessHelper
    {
        private CurrentUser _currentUser;
        private IHttpContextAccessor _context;
        private IWorkflowHelper _workflowHelper;
        private ICalculationHelper _calculationHelper;
        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;

        public ProcessHelper(IWorkflowHelper workflowHelper, ICalculationHelper calculationHelper, IHttpContextAccessor context, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _context = context;
            _currentUser = UserHelper.GetCurrentUser(_context);
            _configuration = configuration;
            _workflowHelper = workflowHelper;
            _serviceScopeFactory = serviceScopeFactory;
            _calculationHelper = calculationHelper;
        }
        public ProcessHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, CurrentUser currentUser)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;

            _currentUser = currentUser;
            _workflowHelper = new WorkflowHelper(configuration, serviceScopeFactory, currentUser);
            _calculationHelper = new CalculationHelper(configuration, serviceScopeFactory, currentUser);
        }
        public async Task Run(OperationType operationType, JObject record, Module module, UserItem appUser, Warehouse warehouse, ProcessTriggerTime triggerTime, BeforeCreateUpdate BeforeCreateUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest, UpdateStageHistory UpdateStageHistory, AfterUpdate AfterUpdate, AfterCreate AfterCreate)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                using (var _processRequestRepository = new ProcessRequestRepository(databaseContext, _configuration))
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _userRepository = new UserRepository(databaseContext, _configuration))
                using (var _processRepository = new ProcessRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
                using (var _settingRepository = new SettingRepository(databaseContext, _configuration))
                {
                    _processRequestRepository.CurrentUser = _moduleRepository.CurrentUser = _userRepository.CurrentUser = _processRepository.CurrentUser = _recordRepository.CurrentUser = _settingRepository.CurrentUser = _currentUser;

                    var requestInsert = await _processRequestRepository.GetByRecordId((int)record["id"], module.Name, OperationType.insert);
                    var requestUpdate = await _processRequestRepository.GetByRecordId((int)record["id"], module.Name, OperationType.update);

                    if ((requestInsert != null && requestInsert.Status == Model.Enums.ProcessStatus.Rejected) || (requestUpdate != null && requestUpdate.Status == Model.Enums.ProcessStatus.Rejected) || (operationType == OperationType.update && requestInsert != null && requestInsert.Status != Model.Enums.ProcessStatus.Rejected))
                        return;



                    var processes = await _processRepository.GetAll(module.Id, appUser.Id, true);
                    processes = processes.Where(x => x.OperationsArray.Contains(operationType.ToString())).ToList();
                    var culture = CultureInfo.CreateSpecificCulture(appUser.TenantLanguage == "tr" ? "tr-TR" : "en-US");

                    if (processes.Count < 1)
                        return;

                    var oldExpense = false;

                    var oldExpenseSetting = await _settingRepository.GetByKeyAsync("old_expense");
                    oldExpense = oldExpenseSetting != null;


                    foreach (var process in processes)
                    {
                        if (process.TriggerTime != triggerTime)
                            return;

                        if ((process.Frequency == WorkflowFrequency.NotSet || process.Frequency == WorkflowFrequency.OneTime) && operationType != OperationType.delete)
                        {
                            var hasLog = await _processRepository.HasLog(process.Id, module.Id, (int)record["id"]);

                            if (hasLog)
                                continue;
                        }

                        var lookupModuleNames = new List<string>();
                        ICollection<Module> lookupModules = null;

                        foreach (var field in module.Fields)
                        {
                            if (!field.Deleted && field.DataType == DataType.Lookup && field.LookupType != "users" && field.LookupType != "profiles" && field.LookupType != "roles" && field.LookupType != "relation" && !lookupModuleNames.Contains(field.LookupType))
                                lookupModuleNames.Add(field.LookupType);
                        }


                        if (lookupModuleNames.Count > 0)
                            lookupModules = await _moduleRepository.GetByNamesBasic(lookupModuleNames);
                        else
                            lookupModules = new List<Module>();

                        lookupModules.Add(Model.Helpers.ModuleHelper.GetFakeUserModule());
                        if (process.ApproverType == ProcessApproverType.DynamicApprover)
                            await _calculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.insert, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest);

                        record = _recordRepository.GetById(module, (int)record["id"], false, lookupModules);

                        if (process.Filters != null && process.Filters.Count > 0)
                        {
                            var filters = process.Filters;
                            var mismatchedCount = 0;

                            foreach (var filter in filters)
                            {
                                var filterField = module.Fields.FirstOrDefault(x => x.Name == filter.Field);

                                if (filterField.DataType == DataType.Lookup && filterField.LookupType != "users" && filterField.LookupType != "profiles" && filterField.LookupType != "roles")
                                    filter.Field = filter.Field + ".id";

                                if (filterField == null || record[filter.Field] == null)
                                {
                                    mismatchedCount++;
                                    continue;
                                }

                                var filterOperator = filter.Operator;
                                var fieldValueString = record[filter.Field].ToString();
                                var filterValueString = filter.Value;
                                double fieldValueNumber;
                                double filterValueNumber;
                                double.TryParse(fieldValueString, out fieldValueNumber);
                                double.TryParse(filterValueString, out filterValueNumber);

                                if (filterField.DataType == DataType.Lookup && filterField.LookupType == "users" && filterField.LookupType == "profiles" && filterField.LookupType == "roles" && filterValueNumber == 0)
                                    filterValueNumber = appUser.Id;

                                switch (filterOperator)
                                {
                                    case Operator.Is:
                                        if (fieldValueString.Trim().ToLower(culture) != filterValueString.Trim().ToLower(culture))
                                            mismatchedCount++;
                                        break;
                                    case Operator.IsNot:
                                        if (fieldValueString.Trim().ToLower(culture) == filterValueString.Trim().ToLower(culture))
                                            mismatchedCount++;
                                        break;
                                    case Operator.Equals:
                                        if (fieldValueNumber != filterValueNumber)
                                            mismatchedCount++;
                                        break;
                                    case Operator.NotEqual:
                                        if (fieldValueNumber == filterValueNumber)
                                            mismatchedCount++;
                                        break;
                                    case Operator.Contains:
                                        if (!(fieldValueString.Contains("|") || filterValueString.Contains("|")))
                                        {
                                            if (!fieldValueString.Trim().ToLower(culture).Contains(filterValueString.Trim().ToLower(culture)))
                                                mismatchedCount++;
                                        }
                                        else
                                        {
                                            var fieldValueStringArray = fieldValueString.Split('|');
                                            var filterValueStringArray = filterValueString.Split('|');

                                            foreach (var filterValueStr in filterValueStringArray)
                                            {
                                                if (!fieldValueStringArray.Contains(filterValueStr))
                                                    mismatchedCount++;
                                            }
                                        }
                                        break;
                                    case Operator.NotContain:
                                        if (!(fieldValueString.Contains("|") || filterValueString.Contains("|")))
                                        {
                                            if (fieldValueString.Trim().ToLower(culture).Contains(filterValueString.Trim().ToLower(culture)))
                                                mismatchedCount++;
                                        }
                                        else
                                        {
                                            var fieldValueStringArray = fieldValueString.Split('|');
                                            var filterValueStringArray = filterValueString.Split('|');

                                            foreach (var filterValueStr in filterValueStringArray)
                                            {
                                                if (fieldValueStringArray.Contains(filterValueStr))
                                                    mismatchedCount++;
                                            }
                                        }
                                        break;
                                    case Operator.StartsWith:
                                        if (!fieldValueString.Trim().ToLower(culture).StartsWith(filterValueString.Trim().ToLower(culture)))
                                            mismatchedCount++;
                                        break;
                                    case Operator.EndsWith:
                                        if (!fieldValueString.Trim().ToLower(culture).EndsWith(filterValueString.Trim().ToLower(culture)))
                                            mismatchedCount++;
                                        break;
                                    case Operator.Empty:
                                        if (!string.IsNullOrWhiteSpace(fieldValueString))
                                            mismatchedCount++;
                                        break;
                                    case Operator.NotEmpty:
                                        if (string.IsNullOrWhiteSpace(fieldValueString))
                                            mismatchedCount++;
                                        break;
                                    case Operator.Greater:
                                        if (fieldValueNumber <= filterValueNumber)
                                            mismatchedCount++;
                                        break;
                                    case Operator.GreaterEqual:
                                        if (fieldValueNumber < filterValueNumber)
                                            mismatchedCount++;
                                        break;
                                    case Operator.Less:
                                        if (fieldValueNumber >= filterValueNumber)
                                            mismatchedCount++;
                                        break;
                                    case Operator.LessEqual:
                                        if (fieldValueNumber > filterValueNumber)
                                            mismatchedCount++;
                                        break;
                                }
                            }

                            if (mismatchedCount > 0)
                            {
                                if (process.TriggerTime == ProcessTriggerTime.Manuel)
                                    throw new ProcessFilterNotMatchException("Filters don't matched");
                                else continue;
                            }
                        }


                        //Set warehouse database name
                        warehouse.DatabaseName = appUser.WarehouseDatabaseName;

                        //checks if fields exist approver, process_date, approver_order, process_status_picklist
                        int fieldCount = 0;
                        bool hasProcessFields = false;
                        foreach (var field in module.Fields)
                        {
                            if (field.Name == "approver" || field.Name == "process_date" || field.Name == "approver_order" || field.Name == "process_status_list")
                                fieldCount++;
                        }

                        if (fieldCount == 4)
                            hasProcessFields = true;


                        var user = new TenantUser();
                        if (process.ApproverType == ProcessApproverType.StaticApprover)
                        {
                            user = await _userRepository.GetById(process.Approvers.First(x => x.Order == 1).UserId);
                            if (hasProcessFields)
                            {
                                using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
                                {
                                    var processStatusPicklist = module.Fields.Single(x => x.Name == "process_status_list");
                                    var processStatus = await picklistRepository.GetById(processStatusPicklist.PicklistId.Value);
                                    record["process_status_list"] = appUser.TenantLanguage == "tr" ? processStatus.Items.Single(x => x.Value == "waiting_for_approval").LabelTr : processStatus.Items.Single(x => x.Value == "waiting_for_approval").LabelEn;
                                    record["approver"] = user.Id;
                                    record["process_date"] = DateTime.Now;
                                    record["approver_order"] = 1;

                                    await _recordRepository.Update(record, module, isUtc: false);
                                }
                            }
                        }
                        else
                        {
                            if (record["custom_approver"].IsNullOrEmpty() && record["custom_approver_2"].IsNullOrEmpty())
                            {
                                var approverFields = process.ApproverField.Split(',');
                                var firstApprover = approverFields[0];
                                var approverFieldName = firstApprover.Split('.')[0];
                                var approverLookupName = firstApprover.Split('.')[1];
                                var approverLookupFieldName = firstApprover.Split('.')[2];

                                var approverField = process.Module.Fields.Single(x => !x.Deleted && x.DataType == DataType.Lookup && x.Name == approverFieldName);
                                Module approverModule;

                                if (approverField.LookupType == process.Module.Name)
                                    approverModule = process.Module;
                                else
                                {

                                    approverModule = await _moduleRepository.GetByNameBasic(approverField.LookupType);
                                }

                                var approverLookupField = approverModule.Fields.Single(x => !x.Deleted && x.Name == approverLookupName);
                                Module approverLookupModule;

                                if (approverLookupField.LookupType != "users")
                                {

                                    approverLookupModule = await _moduleRepository.GetByNameBasic(approverLookupField.LookupType);
                                }

                                else
                                {
                                    approverLookupModule = approverLookupField.LookupType == "profiles" ? Model.Helpers.ModuleHelper.GetFakeProfileModule() : approverLookupField.LookupType == "roles" ? Model.Helpers.ModuleHelper.GetFakeRoleModule(appUser.TenantLanguage) : Model.Helpers.ModuleHelper.GetFakeUserModule();
                                }

                                if (record[firstApprover.Split('.')[0] + "." + approverLookupName] == null)
                                    throw new ProcessFilterNotMatchException("ProcessApproverNotFoundException");

                                var approverUserRecord = _recordRepository.GetById(approverLookupModule, (int)record[firstApprover.Split('.')[0] + "." + approverLookupName], false);
                                var userMail = (string)approverUserRecord[approverLookupFieldName];
                                record["custom_approver"] = userMail;

                                if (approverFields.Length > 1)
                                {
                                    var secondApprover = approverFields[1];
                                    var secondApproverFieldName = secondApprover.Split('.')[0];
                                    var secondApproverLookupName = secondApprover.Split('.')[1];
                                    var secondApproverLookupFieldName = secondApprover.Split('.')[2];


                                    var secondApproverField = process.Module.Fields.Single(x => !x.Deleted && x.DataType == DataType.Lookup && x.Name == secondApproverFieldName);
                                    Module secondApproverModule;

                                    if (secondApproverField.LookupType == process.Module.Name)
                                        secondApproverModule = process.Module;
                                    else
                                    {
                                        secondApproverModule = await _moduleRepository.GetByNameBasic(secondApproverField.LookupType);
                                    }

                                    var secondApproverLookupField = secondApproverModule.Fields.Single(x => !x.Deleted && x.Name == secondApproverLookupName);
                                    Module secondApproverLookupModule;

                                    if (secondApproverLookupField.LookupType != "users")
                                    {
                                        secondApproverLookupModule = await _moduleRepository.GetByNameBasic(secondApproverLookupField.LookupType);
                                    }

                                    else
                                    {
                                        secondApproverLookupModule = secondApproverLookupField.LookupType == "profiles" ? Model.Helpers.ModuleHelper.GetFakeProfileModule() : secondApproverLookupField.LookupType == "roles" ? Model.Helpers.ModuleHelper.GetFakeRoleModule(appUser.TenantLanguage) : Model.Helpers.ModuleHelper.GetFakeUserModule();
                                    }

                                    var secondApproverUserRecord = _recordRepository.GetById(secondApproverLookupModule, (int)record[secondApproverFieldName + "." + secondApproverLookupName], false);
                                    var secondUserMail = (string)secondApproverUserRecord[secondApproverLookupFieldName];
                                    record["custom_approver_2"] = secondUserMail;
                                }

                                await _recordRepository.Update(record, module, isUtc: false);
                                user = await _userRepository.GetByEmail(userMail);
                            }
                            else
                            {
                                var approverMail = (string)record["custom_approver"];
                                user = await _userRepository.GetByEmail(approverMail);
                            }

                            if (hasProcessFields)
                            {
                                using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
                                {
                                    var processStatusPicklist = module.Fields.Single(x => x.Name == "process_status_list");
                                    var processStatus = await picklistRepository.GetById(processStatusPicklist.PicklistId.Value);
                                    record["process_status_list"] = appUser.TenantLanguage == "tr" ? processStatus.Items.Single(x => x.Value == "waiting_for_approval").LabelTr : processStatus.Items.Single(x => x.Value == "waiting_for_approval").LabelEn;
                                    record["approver"] = user.Id;
                                    record["process_date"] = DateTime.Now;
                                    record["approver_order"] = 1;

                                    await _recordRepository.Update(record, module, isUtc: false);
                                }
                            }
                        }

                        var emailData = new Dictionary<string, string>();
                        string domain;

                        domain = "http://{0}.ofisim.com/";

                        var appDomain = "crm";

                        switch (appUser.AppId)
                        {
                            case 2:
                                appDomain = "kobi";
                                break;
                            case 3:
                                appDomain = "asistan";
                                break;
                            case 4:
                                appDomain = "ik";
                                break;
                            case 5:
                                appDomain = "cagri";
                                break;
                        }

                        var subdomain = _configuration.GetSection("AppSettings")["TestMode"] == "true" ? "test" : appDomain;
                        domain = string.Format(domain, subdomain);
                        var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                        using (var _appRepository = new ApplicationRepository(platformDatabaseContext, _configuration, cacheHelper))
                        {
                            var app = await _appRepository.Get(appUser.AppId);
                            if (app != null)
                            {
                                domain = "https://" + app.Setting.AppDomain + "/";
                            }
                        }

                        string url = "";
                        if (module.Name == "timetrackers")
                        {
                            url = domain + "#/app/timetracker?user=" + (int)record["created_by.id"] + "&year=" + (int)record["year"] + "&month=" + (int)record["month"] + "&week=" + (int)record["week"];
                        }
                        else if (process.Module.Name == "masraflar" && !oldExpense)
                        {
                            url = domain + "#/app/crm/expense?user=" + (int)record["created_by.id"] + "&id=" + (int)record["id"];
                        }
                        else
                        {
                            url = domain + "#/app/module/" + module.Name + "?id=" + (int)record["id"];
                        }


                        if (appUser.Culture.Contains("tr"))
                            emailData.Add("ModuleName", module.LabelTrSingular);
                        else
                            emailData.Add("ModuleName", module.LabelEnSingular);

                        emailData.Add("Url", url);
                        emailData.Add("ApproverName", user.FullName);
                        emailData.Add("UserName", (string)record["owner.full_name"]);


                        if (module.Name == "izinler")
                        {
                            await _calculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.insert, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest);

                            if ((bool)record["izin_turu.yillik_izin"] && (int)record["mevcut_kullanilabilir_izin"] -
                                (int)record["hesaplanan_alinacak_toplam_izin"] < 0)
                            {
                                if (appUser.TenantLanguage == "tr")
                                    emailData.Add("ExtraLeave", "Aşağıda detayları bulunan" + " " + (string)record["calisan.ad_soyad"] + " " + "isimli çalışan izin borçlanma talep etmektedir.İzin talebi, yöneticisi olarak sizin onayınıza sunulmuştur.");
                                else
                                    emailData.Add("ExtraLeave", "Employee" + " " + (string)record["calisan.ad_soyad"] + ", with the details below requests leave of absence. It has been submitted for your approval as the manager.");
                            }
                            else
                            {
                                if (appUser.TenantLanguage == "tr")
                                    emailData.Add("ExtraLeave", "Aşağıda detayları bulunan" + " " + (string)record["calisan.ad_soyad"] + " " + "isimli çalışana ait izin talebi, yöneticisi olarak sizin onayınıza sunulmuştur.");
                                else
                                {
                                    emailData.Add("ExtraLeave", "The request form for leave of absence relating to employee" + " " + (string)record["calisan.ad_soyad"] + " " + "with the details below is submitted for your approval as the manager.");
                                }
                            }

                        }
                        else
                        {
                            emailData.Add("ExtraLeave", "");
                        }

                        //TODO if (!string.IsNullOrWhiteSpace(appUser.Culture) && Constants.CULTURES.Contains(appUser.Culture))
                        //TODO	Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(appUser.Culture);

                        //TODO
                        //if (operationType == OperationType.insert)
                        //                  {
                        //                      var notification = new Email(EmailResource.ApprovalProcessCreateNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                        //                      notification.AddRecipient(user.Email);
                        //                      notification.AddToQueue(appUser.TenantId, module.Id, (int)record["id"], appUser: appUser);
                        //                  }
                        //                  else if (operationType == OperationType.update)
                        //                  {
                        //                      var notification = new Email(EmailResource.ApprovalProcessUpdateNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                        //                      notification.AddRecipient(user.Email);
                        //                      notification.AddToQueue(appUser.TenantId, module.Id, (int)record["id"], appUser: appUser);
                        //                  }
                        //                  else if (operationType == OperationType.delete)
                        //                  {

                        //                  }

                        var processRequest = new ProcessRequest
                        {
                            ProcessId = process.Id,
                            RecordId = (int)record["id"],
                            Status = Model.Enums.ProcessStatus.Waiting,
                            OperationType = operationType,
                            ProcessStatusOrder = 1,
                            CreatedById = (int)record["owner.id"],
                            Active = true,
                            Module = module.Name
                        };


                        try
                        {
                            var resultRequestLog = await _processRepository.CreateRequest(processRequest);

                            if (resultRequestLog < 1)
                                ErrorHandler.LogError(new Exception("ProcessRequest cannot be created! Object: " + processRequest.ToJsonString()), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());

                            var newRecord = _recordRepository.GetById(module, (int)record["id"], false);
                            await _workflowHelper.Run(operationType, newRecord, module, appUser, warehouse, BeforeCreateUpdate, UpdateStageHistory, AfterUpdate, AfterCreate);
                            //TODO BPM RUN
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                        }


                        var processLog = new ProcessLog
                        {
                            ProcessId = process.Id,
                            ModuleId = module.Id,
                            RecordId = (int)record["id"]
                        };

                        try
                        {
                            var resultCreateLog = await _processRepository.CreateLog(processLog);

                            if (resultCreateLog < 1)
                                ErrorHandler.LogError(new Exception("ProcessLog cannot be created! Object: " + processLog.ToJsonString()), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                        }
                        catch (Exception ex)
                        {
                            ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
                        }

                    }

                }
            }
        }

        public async Task<Process> CreateEntity(ProcessBindingModel processModel, string tenantLanguage, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, Warehouse warehouse, UserItem appUser)
        {
            var process = new Process
            {
                ModuleId = processModel.ModuleId,
                UserId = processModel.UserId,
                Name = processModel.Name,
                Frequency = processModel.Frequency,
                Active = processModel.Active,
                OperationsArray = processModel.Operations,
                ApproverType = processModel.ApproverType,
                TriggerTime = processModel.TriggerTime,
                ApproverField = processModel.ApproverField,
                Profiles = processModel.Profiles
            };

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                {
                    _picklistRepository.CurrentUser = _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;

                    var module = await _moduleRepository.GetById(processModel.ModuleId);
                    if (processModel.Filters != null && processModel.Filters.Count > 0)
                    {
                        var picklistItemIds = new List<int>();
                        process.Filters = new List<ProcessFilter>();

                        foreach (var filterModel in processModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filterModel.Field);

                            if (field.DataType == DataType.Picklist)
                            {
                                picklistItemIds.Add(int.Parse(filterModel.Value.ToString()));
                            }
                            else if (field.DataType == DataType.Multiselect)
                            {
                                var values = filterModel.Value.ToString().Split(',');

                                foreach (var value in values)
                                {
                                    picklistItemIds.Add(int.Parse(value));
                                }
                            }
                        }

                        ICollection<PicklistItem> picklistItems = null;

                        if (picklistItemIds.Count > 0)
                            picklistItems = await _picklistRepository.FindItems(picklistItemIds);

                        foreach (var filterModel in processModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filterModel.Field);
                            var value = filterModel.Value.ToString();

                            if (field.DataType == DataType.Picklist)
                            {
                                var picklistItem = picklistItems.Single(x => x.Id == int.Parse(filterModel.Value.ToString()));
                                value = tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn;
                            }
                            else if (field.DataType == DataType.Multiselect)
                            {
                                var picklistLabels = new List<string>();

                                var values = filterModel.Value.ToString().Split(',');

                                foreach (var val in values)
                                {
                                    var picklistItem = picklistItems.Single(x => x.Id == int.Parse(val));
                                    picklistLabels.Add(tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn);
                                }

                                value = string.Join("|", picklistLabels);
                            }

                            var filter = new ProcessFilter
                            {
                                Field = filterModel.Field,
                                Operator = filterModel.Operator,
                                Value = value,
                                No = filterModel.No
                            };

                            process.Filters.Add(filter);
                        }
                    }

                    if (processModel.Approvers != null)
                    {
                        process.Approvers = new List<ProcessApprover>();

                        foreach (var processes in processModel.Approvers)
                        {
                            var processApprover = new ProcessApprover
                            {
                                UserId = processes.UserId,
                                Order = processes.Order
                            };

                            process.Approvers.Add(processApprover);
                        }
                    }

                    //otomatik onaycı, onay durumu, işlem tarihi ve seviye ekleme
                    int fieldCount = 0;
                    bool hasProcessFields = false;
                    foreach (var field in module.Fields)
                    {
                        if (field.Name == "approver" || field.Name == "process_date" || field.Name == "approver_order" || field.Name == "process_status_list")
                            fieldCount++;
                    }
                    if (fieldCount == 4)
                        hasProcessFields = true;

                    if (!hasProcessFields)
                    {
                        int picklistId;
                        var picklistResult = await _picklistRepository.GetPicklistByLabelEn("Process Status List");
                        if (picklistResult == null)
                        {
                            var picklist = new PicklistBindingModel { LabelTr = "Onay Durum Listesi", LabelEn = "Process Status List", Items = new List<PicklistItemBindingModel>() };
                            picklist.Items.Add(new PicklistItemBindingModel { LabelTr = "Onay Bekliyor", LabelEn = "Waiting For Approval", Value = "waiting_for_approval", Order = 1 });
                            picklist.Items.Add(new PicklistItemBindingModel { LabelTr = "Onaylandı", LabelEn = "Approved", Value = "approved", Order = 2 });
                            picklist.Items.Add(new PicklistItemBindingModel { LabelTr = "Reddedildi", LabelEn = "Rejected", Value = "rejected", Order = 3 });

                            var picklistEntity = PicklistHelper.CreateEntity(picklist);
                            var result = await picklistRepository.Create(picklistEntity);
                            picklistId = picklistEntity.Id;
                        }
                        else
                        {
                            picklistId = picklistResult.Id;
                        }
                        var moduleChanges = new ModuleChanges();
                        var section = module.Fields.Where(x => x.Name == "created_by").FirstOrDefault().Section;

                        var processPicklist = new Field
                        {
                            Name = "process_status_list",
                            DataType = DataType.Picklist,
                            Deleted = false,
                            DisplayDetail = false,
                            DisplayForm = false,
                            DisplayList = true,
                            Editable = false,
                            InlineEdit = false,
                            LabelEn = "Process Status",
                            LabelTr = "Onay Durumu",
                            Order = (short)(module.Fields.Count + 3),
                            PicklistId = picklistId,
                            PicklistSortorder = SortOrder.Order,
                            Section = section,
                            SectionColumn = 2,
                            ShowLabel = true,
                            Validation = new FieldValidation
                            {
                                Readonly = false,
                                Required = false
                            }
                        };
                        module.Fields.Add(processPicklist);
                        moduleChanges.FieldsAdded.Add(processPicklist);

                        var approverField = new Field
                        {
                            Name = "approver",
                            DataType = DataType.Lookup,
                            Deleted = false,
                            DisplayDetail = false,
                            DisplayForm = false,
                            DisplayList = true,
                            Editable = false,
                            InlineEdit = false,
                            LabelEn = "Approver",
                            LabelTr = "Onaylayıcı",
                            LookupType = "users",
                            LookupSearchType = LookupSearchType.StartsWith,
                            Primary = false,
                            Section = section,
                            SectionColumn = 2,
                            Order = (short)(module.Fields.Count + 1),
                            ShowLabel = true,
                            Validation = new FieldValidation
                            {
                                Readonly = false,
                                Required = false
                            }
                        };
                        module.Fields.Add(approverField);
                        moduleChanges.FieldsAdded.Add(approverField);

                        var processDate = new Field
                        {
                            Name = "process_date",
                            DataType = DataType.Date,
                            Deleted = false,
                            DisplayDetail = false,
                            DisplayForm = false,
                            DisplayList = true,
                            Editable = false,
                            InlineEdit = false,
                            LabelEn = "Process Date",
                            LabelTr = "Onay İşlem Tarihi",
                            Order = (short)(module.Fields.Count + 2),
                            Section = section,
                            SectionColumn = 2,
                            ShowLabel = true,
                            Validation = new FieldValidation
                            {
                                Readonly = false,
                                Required = false
                            }
                        };
                        module.Fields.Add(processDate);
                        moduleChanges.FieldsAdded.Add(processDate);

                        var approverOrder = new Field
                        {
                            Name = "approver_order",
                            DataType = DataType.Number,
                            Deleted = false,
                            DisplayDetail = false,
                            DisplayForm = false,
                            DisplayList = true,
                            Editable = false,
                            InlineEdit = false,
                            LabelEn = "Approver Order",
                            LabelTr = "Onaycı Sırası",
                            Order = (short)(module.Fields.Count + 4),
                            Section = section,
                            SectionColumn = 2,
                            ShowLabel = true,
                            Validation = new FieldValidation
                            {
                                Readonly = false,
                                Required = false
                            }
                        };
                        module.Fields.Add(approverOrder);
                        moduleChanges.FieldsAdded.Add(approverOrder);

                        warehouse.DatabaseName = appUser.WarehouseDatabaseName;

                        await moduleRepository.AlterTable(module, moduleChanges, tenantLanguage);
                        await moduleRepository.Update(module);
                    }

                    return process;
                }
            }

        }

        public async Task UpdateEntity(ProcessBindingModel processModel, Process process, string tenantLanguage)
        {
            process.Name = processModel.Name;
            process.Frequency = processModel.Frequency;
            process.Active = processModel.Active;
            process.OperationsArray = processModel.Operations;
            process.Profiles = processModel.Profiles;
            process.ApproverField = processModel.ApproverField;
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _picklistRepository.CurrentUser = _currentUser;

                    if (processModel.Filters != null && processModel.Filters.Count > 0)
                    {
                        if (process.Filters == null)
                            process.Filters = new List<ProcessFilter>();

                        var module = await _moduleRepository.GetById(processModel.ModuleId);
                        var picklistItemIds = new List<int>();

                        foreach (var filterModel in processModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filterModel.Field);

                            if (field.DataType == DataType.Picklist)
                            {
                                picklistItemIds.Add(int.Parse(filterModel.Value.ToString()));
                            }
                            else if (field.DataType == DataType.Multiselect)
                            {
                                var values = filterModel.Value.ToString().Split(',');

                                foreach (var value in values)
                                {
                                    picklistItemIds.Add(int.Parse(value));
                                }
                            }
                        }

                        ICollection<PicklistItem> picklistItems = null;

                        if (picklistItemIds.Count > 0)
                            picklistItems = await _picklistRepository.FindItems(picklistItemIds);

                        foreach (var filterModel in processModel.Filters)
                        {
                            var field = module.Fields.Single(x => x.Name == filterModel.Field);
                            var value = filterModel.Value.ToString();

                            if (field.DataType == DataType.Picklist)
                            {
                                var picklistItem = picklistItems.Single(x => x.Id == int.Parse(filterModel.Value.ToString()));
                                value = tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn;
                            }
                            else if (field.DataType == DataType.Multiselect)
                            {
                                var picklistLabels = new List<string>();

                                var values = filterModel.Value.ToString().Split(',');

                                foreach (var val in values)
                                {
                                    var picklistItem = picklistItems.Single(x => x.Id == int.Parse(val));
                                    picklistLabels.Add(tenantLanguage == "tr" ? picklistItem.LabelTr : picklistItem.LabelEn);
                                }

                                value = string.Join("|", picklistLabels);
                            }

                            var filter = new ProcessFilter
                            {
                                Field = filterModel.Field,
                                Operator = filterModel.Operator,
                                Value = value,
                                No = filterModel.No
                            };

                            process.Filters.Add(filter);
                        }
                    }

                    if (processModel.Approvers != null)
                    {
                        if (process.Approvers == null)
                            process.Approvers = new List<ProcessApprover>();

                        foreach (var processes in processModel.Approvers)
                        {
                            var processApprover = new ProcessApprover
                            {
                                UserId = processes.UserId,
                                Order = processes.Order
                            };

                            process.Approvers.Add(processApprover);
                        }

                    }
                }
            }
        }

        public async Task ApproveRequest(ProcessRequest request, UserItem appUser, Warehouse warehouse, BeforeCreateUpdate BeforeCreateUpdate, AfterUpdate AfterUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest)
        {
            warehouse.DatabaseName = appUser.WarehouseDatabaseName;
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                //Set warehouse database name
                warehouse.DatabaseName = appUser.WarehouseDatabaseName;

                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                using (var _processRepository = new ProcessRepository(databaseContext, _configuration))
                using (var _userRepository = new UserRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _settingRepository = new SettingRepository(databaseContext, _configuration))
                {
                    _moduleRepository.CurrentUser = _processRepository.CurrentUser = _userRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser = _settingRepository.CurrentUser = _currentUser;

                    var process = await _processRepository.GetById(request.ProcessId);
                    //request.UpdatedById = appUser.LocalId;
                    request.UpdatedAt = DateTime.Now;

                    //recorddaki ilgili process fieldlerini güncellemek için field kontrolü
                    int fieldCount = 0;
                    bool hasProcessFields = false;
                    foreach (var field in process.Module.Fields)
                    {
                        if (field.Name == "approver" || field.Name == "process_date" || field.Name == "approver_order" || field.Name == "process_status_list")
                            fieldCount++;
                    }

                    if (fieldCount == 4)
                        hasProcessFields = true;

                    var oldExpense = false;

                    var oldExpenseSetting = await _settingRepository.GetByKeyAsync("old_expense");
                    oldExpense = oldExpenseSetting != null;

                    var record = new JObject();
                    var lookupModuleNames = new List<string>();
                    ICollection<Module> lookupModules = null;

                    foreach (var field in process.Module.Fields)
                    {
                        if (!field.Deleted && field.DataType == DataType.Lookup && field.LookupType != "users" && field.LookupType != "profiles" && field.LookupType != "roles" && field.LookupType != "relation" && !lookupModuleNames.Contains(field.LookupType))
                            lookupModuleNames.Add(field.LookupType);
                    }

                    if (lookupModuleNames.Count > 0)
                        lookupModules = await _moduleRepository.GetByNamesBasic(lookupModuleNames);
                    else
                        lookupModules = new List<Module>();

                    lookupModules.Add(Model.Helpers.ModuleHelper.GetFakeUserModule());
                    if (process.ApproverType == ProcessApproverType.DynamicApprover)
                        await _calculationHelper.Calculate(request.RecordId, process.Module, appUser, warehouse, OperationType.insert, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest);

                    record = _recordRepository.GetById(process.Module, request.RecordId, false, lookupModules);

                    if ((process.Approvers.Count != request.ProcessStatusOrder && process.ApproverType == ProcessApproverType.StaticApprover) || (process.ApproverType == ProcessApproverType.DynamicApprover && request.ProcessStatusOrder == 1 && process.ApproverField.Split(',').Length > 1))
                    {
                        request.ProcessStatusOrder++;

                        var user = new TenantUser();
                        if (process.ApproverType == ProcessApproverType.StaticApprover)
                        {
                            var nextApproverOrder = request.ProcessStatusOrder;
                            var nextApprover = process.Approvers.FirstOrDefault(x => x.Order == nextApproverOrder);
                            user = await _userRepository.GetById(nextApprover.UserId);
                        }
                        else
                        {
                            var approverMail = (string)record["custom_approver_2"];
                            user = await _userRepository.GetByEmail(approverMail);
                        }

                        if (hasProcessFields)
                        {
                            using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
                            {
                                var processStatusPicklist = process.Module.Fields.Single(x => x.Name == "process_status_list");
                                var processStatus = await picklistRepository.GetById(processStatusPicklist.PicklistId.Value);
                                record["approver"] = user.Id;
                                record["process_status_list"] = appUser.TenantLanguage == "tr" ? processStatus.Items.Single(x => x.Value == "waiting_for_approval").LabelTr : processStatus.Items.Single(x => x.Value == "waiting_for_approval").LabelEn;
                                record["process_date"] = DateTime.Now;
                                record["approver_order"] = request.ProcessStatusOrder;

                                await _recordRepository.Update(record, process.Module, isUtc: false);
                            }
                        }

                        var emailData = new Dictionary<string, string>();
                        string domain;

                        domain = "http://{0}.ofisim.com/";
                        var appDomain = "crm";

                        switch (appUser.AppId)
                        {
                            case 2:
                                appDomain = "kobi";
                                break;
                            case 3:
                                appDomain = "asistan";
                                break;
                            case 4:
                                appDomain = "ik";
                                break;
                            case 5:
                                appDomain = "cagri";
                                break;
                        }

                        var subdomain = _configuration.GetSection("AppSettings")["TestMode"] == "true" ? "test" : appDomain;
                        domain = string.Format(domain, subdomain);
                        var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                        using (var _appRepository = new ApplicationRepository(platformDatabaseContext, _configuration, cacheHelper))
                        {
                            var app = await _appRepository.Get(appUser.AppId);
                            if (app != null)
                            {
                                domain = "https://" + app.Setting.AppDomain + "/";
                            }
                        }

                        string url = "";
                        if (process.Module.Name == "timetrackers")
                        {
                            var findTimetracker = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                            var timetrackerRecord = _recordRepository.Find("timetrackers", findTimetracker)[0];
                            url = domain + "#/app/timetracker?user=" + (int)timetrackerRecord["created_by.id"] + "&year=" + (int)timetrackerRecord["year"] + "&month=" + (int)timetrackerRecord["month"] + "&week=" + (int)timetrackerRecord["week"];
                        }
                        else if (process.Module.Name == "masraflar" && !oldExpense)
                        {
                            var findExpense = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                            var expenseRecord = _recordRepository.Find("masraflar", findExpense)[0];
                            url = domain + "#/app/crm/expense?id=" + (int)expenseRecord["id"];
                        }
                        else
                        {
                            url = domain + "#/app/module/" + process.Module.Name + "?id=" + request.RecordId;
                        }

                        if (appUser.TenantLanguage == "tr")
                            if (appUser.Culture.Contains("tr"))
                                emailData.Add("ModuleName", process.Module.LabelTrSingular);
                            else
                                emailData.Add("ModuleName", process.Module.LabelEnSingular);

                        emailData.Add("Url", url);
                        emailData.Add("ApproverName", user.FullName);

                        if (process.ApproverType == ProcessApproverType.StaticApprover)
                        {
                            emailData.Add("UserName", appUser.FullName);
                        }
                        else
                        {
                            emailData.Add("UserName", (string)record["owner.full_name"]);
                        }

                        //TODO
                        //if (!string.IsNullOrWhiteSpace(user.Culture) && Constants.CULTURES.Contains(user.Culture))
                        //	Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(user.Culture);

                        //if (request.OperationType == OperationType.insert)
                        //{
                        //	var notification = new Email(EmailResource.ApprovalProcessCreateNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                        //	notification.AddRecipient(user.Email);
                        //	notification.AddToQueue(appUser.TenantId, process.Module.Id, request.RecordId, appUser: appUser);
                        //}
                        //else if (request.OperationType == OperationType.update)
                        //{
                        //	var notification = new Email(EmailResource.ApprovalProcessUpdateNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                        //	notification.AddRecipient(user.Email);
                        //	notification.AddToQueue(appUser.TenantId, process.Module.Id, request.RecordId, appUser: appUser);
                        //}
                        //else if (request.OperationType == OperationType.delete)
                        //{

                        //}

                    }
                    else
                    {

                        if (request.OperationType == OperationType.delete)
                        {
                            record = _recordRepository.GetById(process.Module, request.RecordId, !appUser.HasAdminProfile);
                            await _recordRepository.Delete(record, process.Module);
                        }

                        int processOrder = request.ProcessStatusOrder + 1;

                        string beforeCc = "";
                        var recordMail = !record["custom_approver"].IsNullOrEmpty() ? record["custom_approver"].ToString() : "";

                        if (!record["process_status_order"].IsNullOrEmpty() && processOrder != 1 && recordMail.Contains("@etiya.com") && process.Module.Name == "ise_alim_talepleri")
                        {
                            switch (processOrder)
                            {
                                case 2:
                                    beforeCc = (string)record["custom_approver"] + "," + "hr@etiya.com";
                                    break;
                                case 3:
                                    beforeCc = record["custom_approver_2"] + "," + record["custom_approver"] + "," + "hr@etiya.com";
                                    break;
                                case 4:
                                    beforeCc = record["custom_approver_3"] + "," + record["custom_approver_2"] + "," + record["custom_approver"] + "," + "hr@etiya.com";
                                    break;
                                case 5:
                                    beforeCc = record["custom_approver_4"] + "" + record["custom_approver_3"] + "," + record["custom_approver_2"] + "," + record["custom_approver"] + "," + "hr@etiya.com";
                                    break;
                            }
                        }
                        if (!record["custom_approver_" + processOrder].IsNullOrEmpty())
                        {
                            request.ProcessStatusOrder++;
                            var user = new TenantUser();
                            var approverMail = (string)record["custom_approver_" + processOrder];
                            user = await _userRepository.GetByEmail(approverMail);
                            var emailData = new Dictionary<string, string>();
                            string domain;

                            domain = "http://{0}.ofisim.com/";
                            var appDomain = "crm";

                            switch (appUser.AppId)
                            {
                                case 2:
                                    appDomain = "kobi";
                                    break;
                                case 3:
                                    appDomain = "asistan";
                                    break;
                                case 4:
                                    appDomain = "ik";
                                    break;
                                case 5:
                                    appDomain = "cagri";
                                    break;
                            }

                            var subdomain = _configuration.GetSection("AppSettings")["TestMode"] == "true" ? "test" : appDomain;
                            domain = string.Format(domain, subdomain);
                            var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                            using (var _appRepository = new ApplicationRepository(platformDatabaseContext, _configuration, cacheHelper))
                            {
                                var app = await _appRepository.Get(appUser.AppId);
                                if (app != null)
                                {
                                    domain = "https://" + app.Setting.AppDomain + "/";
                                }
                            }

                            string url = "";
                            if (process.Module.Name == "timetrackers")
                            {
                                var findTimetracker = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                                var timetrackerRecord = _recordRepository.Find("timetrackers", findTimetracker)[0];
                                url = domain + "#/app/timetracker?user=" + (int)timetrackerRecord["created_by.id"] + "&year=" + (int)timetrackerRecord["year"] + "&month=" + (int)timetrackerRecord["month"] + "&week=" + (int)timetrackerRecord["week"];
                            }
                            else if (process.Module.Name == "masraflar" && !oldExpense)
                            {
                                var findExpense = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                                var expenseRecord = _recordRepository.Find("masraflar", findExpense)[0];
                                url = domain + "#/app/crm/expense?user=" + (int)expenseRecord["created_by.id"] + "&id=" + (int)expenseRecord["id"];
                            }
                            else
                            {
                                url = domain + "#/app/module/" + process.Module.Name + "?id=" + request.RecordId;
                            }

                            if (appUser.Culture.Contains("tr"))
                                emailData.Add("ModuleName", process.Module.LabelTrSingular);
                            else
                                emailData.Add("ModuleName", process.Module.LabelEnSingular);

                            emailData.Add("Url", url);
                            emailData.Add("ApproverName", user.FullName);

                            if (process.ApproverType == ProcessApproverType.StaticApprover)
                            {
                                emailData.Add("UserName", appUser.FullName);
                            }
                            else
                            {
                                emailData.Add("UserName", (string)record["owner.full_name"]);
                            }

                            //TODO
                            //if (!string.IsNullOrWhiteSpace(user.Culture) && Constants.CULTURES.Contains(user.Culture))
                            //	Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(user.Culture);

                            //if (request.OperationType == OperationType.insert)
                            //{
                            //	var notification = new Email(EmailResource.ApprovalProcessCreateNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                            //	notification.AddRecipient(user.Email);
                            //	notification.AddToQueue(appUser.TenantId, process.Module.Id, request.RecordId, appUser: appUser, cc: beforeCc);
                            //}
                            //else if (request.OperationType == OperationType.update)
                            //{
                            //	var notification = new Email(EmailResource.ApprovalProcessUpdateNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                            //	notification.AddRecipient(user.Email);
                            //	notification.AddToQueue(appUser.TenantId, process.Module.Id, request.RecordId, appUser: appUser, cc: beforeCc);
                            //}
                            //else if (request.OperationType == OperationType.delete)
                            //{

                            //}
                        }
                        else
                        {
                            var user = await _userRepository.GetById(request.CreatedById);
                            request.Status = Model.Enums.ProcessStatus.Approved;
                            request.Active = false;
                            var emailData = new Dictionary<string, string>();
                            string domain;

                            domain = "http://{0}.ofisim.com/";
                            var appDomain = "crm";

                            switch (appUser.AppId)
                            {
                                case 2:
                                    appDomain = "kobi";
                                    break;
                                case 3:
                                    appDomain = "asistan";
                                    break;
                                case 4:
                                    appDomain = "ik";
                                    break;
                                case 5:
                                    appDomain = "cagri";
                                    break;
                            }

                            var subdomain = _configuration.GetSection("AppSettings")["TestMode"] == "true" ? "test" : appDomain;
                            domain = string.Format(domain, subdomain);
                            var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                            using (var _appRepository = new ApplicationRepository(platformDatabaseContext, _configuration, cacheHelper))
                            {
                                var app = await _appRepository.Get(appUser.AppId);
                                if (app != null)
                                {
                                    domain = "https://" + app.Setting.AppDomain + "/";
                                }
                            }

                            string url = "";
                            if (process.Module.Name == "timetrackers")
                            {
                                var findTimetracker = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                                var timetrackerRecord = _recordRepository.Find("timetrackers", findTimetracker);
                                url = domain + "#/app/timetracker?user=" + (int)timetrackerRecord.First()["created_by"] + "&year=" + (int)timetrackerRecord.First()["year"] + "&month=" + (int)timetrackerRecord.First()["month"] + "&week=" + (int)timetrackerRecord.First()["week"];
                            }
                            else if (process.Module.Name == "masraflar" && !oldExpense)
                            {
                                var findExpense = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                                var expenseRecord = _recordRepository.Find("masraflar", findExpense);
                                url = domain + "#/app/crm/expense?user=" + (int)expenseRecord.First()["created_by"] + "&id=" + (int)expenseRecord.First()["id"];
                            }
                            else
                            {
                                url = domain + "#/app/module/" + process.Module.Name + "?id=" + request.RecordId;
                            }

                            if (appUser.TenantLanguage == "tr")
                                if (appUser.Culture.Contains("tr"))
                                    emailData.Add("ModuleName", process.Module.LabelTrSingular);
                                else
                                    emailData.Add("ModuleName", process.Module.LabelEnSingular);

                            emailData.Add("Url", url);
                            emailData.Add("UserName", user.FullName);

                            //TODO
                            //if (!string.IsNullOrWhiteSpace(user.Culture) && Constants.CULTURES.Contains(user.Culture))
                            //	Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(user.Culture);

                            //var notification = new Email(EmailResource.ApprovalProcessApproveNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                            //notification.AddRecipient(user.Email);
                            //notification.AddToQueue(appUser.TenantId, process.Module.Id, request.RecordId, appUser: appUser);
                        }
                    }

                    if (hasProcessFields)
                    {
                        using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
                        {
                            var processStatusPicklist = process.Module.Fields.Single(x => x.Name == "process_status_list");
                            var processStatus = await picklistRepository.GetById(processStatusPicklist.PicklistId.Value);
                            record["process_status_list"] = appUser.TenantLanguage == "tr" ? processStatus.Items.Single(x => x.Value == "approved").LabelTr : processStatus.Items.Single(x => x.Value == "approved").LabelEn;
                            record["process_date"] = DateTime.Now;
                            record["approver_order"] = request.ProcessStatusOrder;

                            await _recordRepository.Update(record, process.Module, isUtc: false);
                        }
                    }
                }
            }
        }

        public async Task RejectRequest(ProcessRequest request, string message, UserItem appUser, Warehouse warehouse)
        {
            warehouse.DatabaseName = appUser.WarehouseDatabaseName;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                using (var _processRepository = new ProcessRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
                using (var _userRepository = new UserRepository(databaseContext, _configuration))
                using (var _settingRepository = new SettingRepository(databaseContext, _configuration))
                {
                    var oldExpense = false;

                    var oldExpenseSetting = await _settingRepository.GetByKeyAsync("old_expense");
                    oldExpense = oldExpenseSetting != null;

                    _processRepository.CurrentUser = _recordRepository.CurrentUser = _userRepository.CurrentUser = _currentUser = _settingRepository.CurrentUser = _currentUser;

                    var process = await _processRepository.GetById(request.ProcessId);
                    var record = _recordRepository.GetById(process.Module, request.RecordId);

                    //recorddaki ilgili process fieldlerini güncellemek için field kontrolü
                    int fieldCount = 0;
                    bool hasProcessFields = false;
                    foreach (var field in process.Module.Fields)
                    {
                        if (field.Name == "approver" || field.Name == "process_date" || field.Name == "approver_order" || field.Name == "process_status_list")
                            fieldCount++;
                    }

                    if (fieldCount == 4)
                        hasProcessFields = true;

                    if (hasProcessFields)
                    {
                        using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
                        {
                            var processStatusPicklist = process.Module.Fields.Single(x => x.Name == "process_status_list");
                            var processStatus = await picklistRepository.GetById(processStatusPicklist.PicklistId.Value);
                            record["process_status_list"] = appUser.TenantLanguage == "tr" ? processStatus.Items.Single(x => x.Value == "rejected").LabelTr : processStatus.Items.Single(x => x.Value == "rejected").LabelEn;
                            record["process_date"] = DateTime.Now;

                            await _recordRepository.Update(record, process.Module, isUtc: false);
                        }
                    }

                    string beforeCc = "";
                    var recordMail = !record["custom_approver"].IsNullOrEmpty() ? record["custom_approver"].ToString() : "";
                    if (!record["process_status_order"].IsNullOrEmpty() && (int)record["process_status_order"] != 1 && recordMail.Contains("@etiya.com") && process.Module.Name == "ise_alim_talepleri")
                    {
                        switch ((int)record["process_status_order"])
                        {
                            case 2:
                                beforeCc = (string)record["custom_approver"] + "," + "hr@etiya.com";
                                break;
                            case 3:
                                beforeCc = record["custom_approver_2"] + "," + record["custom_approver"] + "," + "hr@etiya.com";
                                break;
                            case 4:
                                beforeCc = record["custom_approver_3"] + "," + record["custom_approver_2"] + "," + record["custom_approver"] + "," + "hr@etiya.com";
                                break;
                            case 5:
                                beforeCc = record["custom_approver_4"] + "" + record["custom_approver_3"] + "," + record["custom_approver_2"] + "," + record["custom_approver"] + "," + "hr@etiya.com";
                                break;
                        }
                    }


                    var user = await _userRepository.GetById(request.CreatedById);
                    request.Status = Model.Enums.ProcessStatus.Rejected;
                    request.ProcessStatusOrder = 0;
                    //request.UpdatedById = appUser.LocalId;
                    request.UpdatedAt = DateTime.Now;
                    var emailData = new Dictionary<string, string>();
                    string domain;

                    domain = "http://{0}.ofisim.com/";
                    var appDomain = "crm";

                    switch (appUser.AppId)
                    {
                        case 2:
                            appDomain = "kobi";
                            break;
                        case 3:
                            appDomain = "asistan";
                            break;
                        case 4:
                            appDomain = "ik";
                            break;
                        case 5:
                            appDomain = "cagri";
                            break;
                    }

                    var subdomain = _configuration.GetSection("AppSettings")["TestMode"] == "true" ? "test" : appDomain;
                    domain = string.Format(domain, subdomain);
                    var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                    using (var _appRepository = new ApplicationRepository(platformDatabaseContext, _configuration, cacheHelper))
                    {
                        var app = await _appRepository.Get(appUser.AppId);
                        if (app != null)
                        {
                            domain = "https://" + app.Setting.AppDomain + "/";
                        }
                    }

                    string url = "";
                    if (process.Module.Name == "timetrackers")
                    {
                        var findTimetracker = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                        var timetrackerRecord = _recordRepository.Find("timetrackers", findTimetracker);
                        url = domain + "#/app/timetracker?user=" + (int)timetrackerRecord.First()["created_by"] + "&year=" + (int)timetrackerRecord.First()["year"] + "&month=" + (int)timetrackerRecord.First()["month"] + "&week=" + (int)timetrackerRecord.First()["week"];
                    }
                    else if (process.Module.Name == "masraflar" && !oldExpense)
                    {
                        var findExpense = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                        var expenseRecord = _recordRepository.Find("masraflar", findExpense);
                        url = domain + "#/app/crm/expense?user=" + (int)expenseRecord.First()["created_by"] + "&id=" + (int)expenseRecord.First()["id"];
                    }
                    else
                    {
                        url = domain + "#/app/module/" + process.Module.Name + "?id=" + request.RecordId;
                    }

                    if (appUser.TenantLanguage == "tr")
                        if (appUser.Culture.Contains("tr"))
                            emailData.Add("ModuleName", process.Module.LabelTrSingular);
                        else
                            emailData.Add("ModuleName", process.Module.LabelEnSingular);

                    emailData.Add("Url", url);
                    emailData.Add("UserName", user.FullName);
                    emailData.Add("Message", message);
                    emailData.Add("RejectedUser", appUser.FullName);

                    //TODO
                    //if (!string.IsNullOrWhiteSpace(user.Culture) && Constants.CULTURES.Contains(user.Culture))
                    //	Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(user.Culture);

                    //if (request.OperationType == OperationType.insert)
                    //{
                    //	var notification = new Email(EmailResource.ApprovalProcessRejectNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                    //	notification.AddRecipient(user.Email);
                    //	notification.AddToQueue(appUser.TenantId, process.Module.Id, request.RecordId, appUser: appUser, cc: beforeCc);
                    //}
                    //else if (request.OperationType == OperationType.update)
                    //{
                    //	var notification = new Email(EmailResource.ApprovalProcessUpdateRejectNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                    //	notification.AddRecipient(user.Email);
                    //	notification.AddToQueue(appUser.TenantId, process.Module.Id, request.RecordId, appUser: appUser);
                    //}

                    //TODO 
                    //if (process.Module.Name == "izinler")
                    //    await _calculationHelper.Calculate(request.RecordId, process.Module, appUser, warehouse, OperationType.insert, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest);
                }
            }
        }

        public async Task SendToApprovalAgain(ProcessRequest request, UserItem appUser, Warehouse warehouse, BeforeCreateUpdate BeforeCreateUpdate, AfterUpdate AfterUpdate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest)
        {

            warehouse.DatabaseName = appUser.WarehouseDatabaseName;
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                using (var _processRepository = new ProcessRepository(databaseContext, _configuration))
                using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
                using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                using (var _userRepository = new UserRepository(databaseContext, _configuration))
                using (var _settingRepository = new SettingRepository(databaseContext, _configuration))
                {

                    var oldExpense = false;

                    var oldExpenseSetting = await _settingRepository.GetByKeyAsync("old_expense");
                    oldExpense = oldExpenseSetting != null;

                    _moduleRepository.CurrentUser = _processRepository.CurrentUser = _recordRepository.CurrentUser = _userRepository.CurrentUser = _currentUser = _settingRepository.CurrentUser = _currentUser;
                    var process = await _processRepository.GetById(request.ProcessId);

                    request.ProcessStatusOrder++;
                    request.Status = Model.Enums.ProcessStatus.Waiting;


                    var user = new TenantUser();
                    var record = new JObject();
                    if (process.ApproverType == ProcessApproverType.StaticApprover)
                    {
                        var nextApproverOrder = request.ProcessStatusOrder;
                        var nextApprover = process.Approvers.FirstOrDefault(x => x.Order == nextApproverOrder);
                        user = await _userRepository.GetById(nextApprover.UserId);
                    }
                    else
                    {
                        var lookupModuleNames = new List<string>();
                        ICollection<Module> lookupModules = null;

                        foreach (var field in process.Module.Fields)
                        {
                            if (!field.Deleted && field.DataType == DataType.Lookup && field.LookupType != "users" && field.LookupType != "profiles" && field.LookupType != "roles" && field.LookupType != "relation" && !lookupModuleNames.Contains(field.LookupType))
                                lookupModuleNames.Add(field.LookupType);
                        }


                        if (lookupModuleNames.Count > 0)
                            lookupModules = await _moduleRepository.GetByNamesBasic(lookupModuleNames);
                        else
                            lookupModules = new List<Module>();

                        lookupModules.Add(Model.Helpers.ModuleHelper.GetFakeUserModule());
                        if (process.ApproverType == ProcessApproverType.DynamicApprover)
                            await _calculationHelper.Calculate(request.RecordId, process.Module, appUser, warehouse, OperationType.insert, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest);

                        record = _recordRepository.GetById(process.Module, request.RecordId, false, lookupModules);
                        var approverMail = (string)record["custom_approver"];
                        user = await _userRepository.GetByEmail(approverMail);

                    }

                    //recorddaki ilgili process fieldlerini güncellemek için field kontrolü
                    int fieldCount = 0;
                    bool hasProcessFields = false;
                    foreach (var field in process.Module.Fields)
                    {
                        if (field.Name == "approver" || field.Name == "process_date" || field.Name == "approver_order" || field.Name == "process_status_list")
                            fieldCount++;
                    }

                    if (fieldCount == 4)
                        hasProcessFields = true;

                    if (hasProcessFields)
                    {
                        using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
                        {
                            var processStatusPicklist = process.Module.Fields.Single(x => x.Name == "process_status_list");
                            var processStatus = await picklistRepository.GetById(processStatusPicklist.PicklistId.Value);
                            record["process_status_list"] = appUser.TenantLanguage == "tr" ? processStatus.Items.Single(x => x.Value == "waiting_for_approval").LabelTr : processStatus.Items.Single(x => x.Value == "waiting_for_approval").LabelEn;
                            record["process_date"] = DateTime.Now;
                            record["approver"] = user.Id;
                            record["approver_order"] = 1;
                            record["id"] = request.RecordId;

                            await _recordRepository.Update(record, process.Module, isUtc: false);
                        }
                    }

                    var emailData = new Dictionary<string, string>();
                    string domain;

                    domain = "http://{0}.ofisim.com/";
                    var appDomain = "crm";

                    switch (appUser.AppId)
                    {
                        case 2:
                            appDomain = "kobi";
                            break;
                        case 3:
                            appDomain = "asistan";
                            break;
                        case 4:
                            appDomain = "ik";
                            break;
                        case 5:
                            appDomain = "cagri";
                            break;
                    }

                    var subdomain = _configuration.GetSection("AppSettings")["TestMode"] == "true" ? "test" : appDomain;
                    domain = string.Format(domain, subdomain);
                    var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                    using (var _appRepository = new ApplicationRepository(platformDatabaseContext, _configuration, cacheHelper))
                    {
                        var app = await _appRepository.Get(appUser.AppId);
                        if (app != null)
                        {
                            domain = "https://" + app.Setting.AppDomain + "/";
                        }
                    }

                    string url = "";
                    if (process.Module.Name == "timetrackers")
                    {
                        using (var recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
                        {
                            var findTimetracker = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                            var timetrackerRecord = recordRepository.Find("timetrackers", findTimetracker);
                            url = domain + "#/app/timetracker?user=" + (int)timetrackerRecord.First()["created_by"] + "&year=" + (int)timetrackerRecord.First()["year"] + "&month=" + (int)timetrackerRecord.First()["month"] + "&week=" + (int)timetrackerRecord.First()["week"];
                        }
                    }
                    else if (process.Module.Name == "masraflar" && !oldExpense)
                    {
                        var findExpense = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)request.RecordId, No = 1 } }, Limit = 9999 };
                        var expenseRecord = _recordRepository.Find("masraflar", findExpense);
                        url = domain + "#/app/crm/expense?user=" + (int)expenseRecord.First()["created_by"] + "&id=" + (int)expenseRecord.First()["id"];
                    }
                    else
                    {
                        url = domain + "#/app/module/" + process.Module.Name + "?id=" + request.RecordId;
                    }

                    if (appUser.TenantLanguage == "tr")
                        emailData.Add("ModuleName", process.Module.LabelTrSingular);
                    else
                        emailData.Add("ModuleName", process.Module.LabelEnSingular);

                    emailData.Add("Url", url);
                    emailData.Add("ApproverName", user.FullName);
                    if (process.ApproverType == ProcessApproverType.StaticApprover)
                    {
                        emailData.Add("UserName", appUser.FullName);
                    }
                    else
                    {
                        emailData.Add("UserName", (string)record["owner.full_name"]);
                    }

                    //TODO
                    //if (!string.IsNullOrWhiteSpace(user.Culture) && Constants.CULTURES.Contains(user.Culture))
                    //	Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(user.Culture);

                    //if (request.OperationType == OperationType.insert)
                    //{
                    //	var notification = new Email(EmailResource.ApprovalProcessCreateNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                    //	notification.AddRecipient(user.Email);
                    //	notification.AddToQueue(appUser.TenantId, process.Module.Id, request.RecordId, appUser: appUser);
                    //}
                    //else if (request.OperationType == OperationType.update)
                    //{
                    //	var notification = new Email(EmailResource.ApprovalProcessUpdateNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                    //	notification.AddRecipient(user.Email);
                    //	notification.AddToQueue(appUser.TenantId, process.Module.Id, request.RecordId, appUser: appUser);
                    //}
                    //else if (request.OperationType == OperationType.delete)
                    //{

                    //}
                    if (process.Module.Name == "izinler")
                        await _calculationHelper.Calculate(request.RecordId, process.Module, appUser, warehouse, OperationType.insert, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest);
                }
            }
        }

        //public static async Task SendToApprovalApprovedRequest(OperationType operationType, JObject record, UserItem appUser, Warehouse warehouse)
        //{
        //    using (var databaseContext = new TenantDBContext(appUser.TenantId))
        //    {
        //        using (var recordRepository = new RecordRepository(databaseContext, warehouse, configuration))
        //        {
        //            warehouse.DatabaseName = appUser.WarehouseDatabaseName;

        //            using (var processRequestRepository = new ProcessRequestRepository(databaseContext, configuration))
        //            {
        //                var requestEntity = await processRequestRepository.GetByRecordId((int)record["id"], operationType);
        //                using (var processRepository = new ProcessRepository(databaseContext, configuration))
        //                {
        //                    var process = await processRepository.GetById(requestEntity.ProcessId);

        //                    requestEntity.ProcessStatusOrder = 1;
        //                    requestEntity.Status = Model.Enums.ProcessStatus.Waiting;

        //                    using (var userRepository = new UserRepository(databaseContext, configuration))
        //                    {
        //                        var nextApproverOrder = requestEntity.ProcessStatusOrder;
        //                        var nextApprover = process.Approvers.FirstOrDefault(x => x.Order == nextApproverOrder);
        //                        var user = await userRepository.GetById(nextApprover.UserId);

        //                        var emailData = new Dictionary<string, string>();
        //                        string domain;

        //                        domain = "https://{0}.ofisim.com/";
        //                        var appDomain = "crm";

        //                        switch (appUser.AppId)
        //                        {
        //                            case 2:
        //                                appDomain = "kobi";
        //                                break;
        //                            case 3:
        //                                appDomain = "asistan";
        //                                break;
        //                            case 4:
        //                                appDomain = "ik";
        //                                break;
        //                            case 5:
        //                                appDomain = "cagri";
        //                                break;
        //                        }

        //                        var subdomain = configuration.GetSection("AppSettings")["TestMode") == "true" ? "test" : appDomain;
        //                        domain = string.Format(domain, subdomain);

        //                        //domain = "http://localhost:5554/";
        //                        string url = "";
        //                        if (process.Module.Name == "timetrackers")
        //                        {
        //                            var findTimetracker = new FindRequest { Filters = new List<Filter> { new Filter { Field = "id", Operator = Operator.Equals, Value = (int)requestEntity.RecordId, No = 1 } }, Limit = 9999 };
        //                            var timetrackerRecord = recordRepository.Find("timetrackers", findTimetracker);
        //                            url = domain + "#/app/timetracker?user=" + (int)timetrackerRecord.First()["created_by"] + "&year=" + (int)timetrackerRecord.First()["year"] + "&month=" + (int)timetrackerRecord.First()["month"] + "&week=" + (int)timetrackerRecord.First()["week"];
        //                        }
        //                        else
        //                        {
        //                            url = domain + "#/app/module/" + process.Module.Name + "?id=" + requestEntity.RecordId;
        //                        }

        //                        if (appUser.TenantLanguage == "tr")
        //                            emailData.Add("ModuleName", process.Module.LabelTrSingular);
        //                        else
        //                            emailData.Add("ModuleName", process.Module.LabelEnSingular);

        //                        emailData.Add("Url", url);
        //                        emailData.Add("ApproverName", user.FullName);
        //                        emailData.Add("UserName", appUser.UserName);

        //                        if (!string.IsNullOrWhiteSpace(user.Culture) && Constants.CULTURES.Contains(user.Culture))
        //                            Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(user.Culture);

        //                        var notification = new Email(EmailResource.ApprovalProcessUpdateNotification, Thread.CurrentThread.CurrentCulture.Name, emailData, appUser.AppId, appUser);
        //                        notification.AddRecipient(user.Email);
        //                        notification.AddToQueue(appUser.TenantId, process.Module.Id, (int)record["id"], appUser: appUser);
        //                    }

        //                    await processRequestRepository.Update(requestEntity);
        //                }

        //            }
        //        }
        //    }
        //}

        public async Task AfterCreateProcess(ProcessRequest request, UserItem appUser, Warehouse warehouse, BeforeCreateUpdate BeforeCreateUpdate, UpdateStageHistory UpdateStageHistory, AfterUpdate AfterUpdate, AfterCreate AfterCreate, GetAllFieldsForFindRequest GetAllFieldsForFindRequest)
        {
            warehouse.DatabaseName = appUser.WarehouseDatabaseName;

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
                using (var _processRepository = new ProcessRepository(databaseContext, _configuration))
                {
                    _recordRepository.CurrentUser = _processRepository.CurrentUser = _currentUser;
                    var process = await _processRepository.GetById(request.ProcessId);

                    var record = _recordRepository.GetById(process.Module, request.RecordId, false);
                    await _workflowHelper.Run(request.OperationType, record, process.Module, appUser, warehouse, BeforeCreateUpdate, UpdateStageHistory, AfterUpdate, AfterCreate);
                    //TODO BPM RUN

                    if (process.Module.Name == "izinler")
                        await _calculationHelper.Calculate(request.RecordId, process.Module, appUser, warehouse, OperationType.update, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest);
                }
            }
        }

        [Serializable]
        public class ProcessFilterNotMatchException : Exception
        {
            public ProcessFilterNotMatchException() { }
            public ProcessFilterNotMatchException(string message) : base(message) { }
            public ProcessFilterNotMatchException(string message, Exception inner) : base(message, inner) { }
            protected ProcessFilterNotMatchException(
                System.Runtime.Serialization.SerializationInfo info,
                System.Runtime.Serialization.StreamingContext context) : base(info, context) { }
        }
    }
}