using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using RecordHelper = PrimeApps.App.Helpers.RecordHelper;

namespace PrimeApps.App.Bpm.Steps
{
    public class DataUpdateStep : StepBodyAsync
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;

        public string Request { get; set; }
        public string Response { get; set; }

        public DataUpdateStep(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            try
            {
                var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
                previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

                if (context == null)
                    throw new NullReferenceException();

                if (context.Workflow.Reference == null)
                    throw new NullReferenceException();

                var appUser = JsonConvert.DeserializeObject<UserItem>(context.Workflow.Reference);
                var currentUser = new CurrentUser { };

                currentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

                var request = Request != null ? JsonConvert.DeserializeObject<JObject>(Request.Replace("\\", "")) : null;

                if (request == null || request.IsNullOrEmpty())
                    throw new Exception("Request cannot be null.");

                var dataUpdateRequest = request["field_update"];

                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                    var platformDatabaseContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                   // var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                    using (var platformWarehouseRepository = new PlatformWarehouseRepository(platformDatabaseContext, _configuration))
                    using (var analyticRepository = new AnalyticRepository(databaseContext, _configuration))
                    {
                        platformWarehouseRepository.CurrentUser = analyticRepository.CurrentUser = currentUser;

                        var warehouse = new Model.Helpers.Warehouse(analyticRepository, _configuration);
                        var warehouseEntity = await platformWarehouseRepository.GetByTenantId(currentUser.TenantId);

                        if (warehouseEntity != null)
                            warehouse.DatabaseName = warehouseEntity.DatabaseName;
                        else
                            warehouse.DatabaseName = "0";

                        using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
                        using (var recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
                        using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
                        using (var profileRepository = new ProfileRepository(databaseContext, _configuration))
                        using (var tagRepository = new TagRepository(databaseContext, _configuration))
                        using (var settingRepository = new SettingRepository(databaseContext, _configuration))
                        using (var recordHelper = new RecordHelper(_configuration, _serviceScopeFactory, currentUser))
                        {
                            picklistRepository.CurrentUser = moduleRepository.CurrentUser = recordRepository.CurrentUser = profileRepository.CurrentUser = tagRepository.CurrentUser = settingRepository.CurrentUser = currentUser;

                            var data = JObject.FromObject(context.Workflow.Data);

                            if (dataUpdateRequest == null || dataUpdateRequest.IsNullOrEmpty() || data["module_id"].IsNullOrEmpty())
                                throw new MissingFieldException("Cannot find child data");

                            var fieldUpdate = new BpmDataUpdate();
                            fieldUpdate.Module = await moduleRepository.GetById(dataUpdateRequest["module_id"].Value<int>());
                            fieldUpdate.Field = await moduleRepository.GetField(dataUpdateRequest["field_id"].Value<int>());
                            fieldUpdate.Value = dataUpdateRequest["currentValue"].IsNullOrEmpty() ? dataUpdateRequest["value"].Value<string>() : dataUpdateRequest["currentValue"].Value<string>();

                            var moduleId = data["module_id"].ToObject<int>();
                            var module = await moduleRepository.GetById(moduleId);

                            var record = data["record"].ToObject<JObject>();


                            var fieldUpdateRecords = new Dictionary<string, int>();
                            var isDynamicUpdate = false;
                            var type = 0;
                            var firstModule = string.Empty;
                            string secondModule;

                            if (fieldUpdate.Module.Name.Contains(','))
                            {
                                isDynamicUpdate = true;
                                var modules = fieldUpdate.Module.Name.Split(',');
                                firstModule = modules[0];
                                secondModule = modules[1];

                                if (firstModule == module.Name && firstModule == secondModule)
                                {
                                    type = 1;
                                    fieldUpdateRecords.Add(secondModule, (int)record["id"]);
                                }
                                else
                                {
                                    if (firstModule == module.Name && firstModule != secondModule)
                                    {
                                        type = 2;
                                        var secondModuleName = module.Fields.Where(q => q.Name == secondModule).FirstOrDefault().LookupType;

                                        foreach (var field in module.Fields)
                                        {
                                            if (field.LookupType != null && field.LookupType == secondModuleName && (!record[fieldUpdate.Value].IsNullOrEmpty() || !record[fieldUpdate.Value + ".id"].IsNullOrEmpty()))
                                            {
                                                if (fieldUpdateRecords.Count < 1)
                                                    fieldUpdateRecords.Add(secondModule, (int)record[field.Name + ".id"]);
                                            }
                                        }
                                    }
                                    else if (firstModule != module.Name && secondModule == module.Name)
                                    {
                                        type = 3;
                                        var firstModuleName = module.Fields.Where(q => q.Name == firstModule).FirstOrDefault().LookupType;

                                        foreach (var field in module.Fields)
                                        {
                                            if (field.LookupType != null && field.LookupType == firstModule && (!record[fieldUpdate.Value].IsNullOrEmpty() || !record[firstModule + "." + fieldUpdate.Value].IsNullOrEmpty()))
                                            {
                                                if (fieldUpdateRecords.Count < 1)
                                                    fieldUpdateRecords.Add(secondModule, (int)record["id"]);
                                            }
                                        }
                                    }
                                    else if (firstModule != module.Name && secondModule != module.Name)
                                    {
                                        type = 4;
                                        var firstModuleName = module.Fields.Where(q => q.Name == firstModule).FirstOrDefault().LookupType;
                                        var secondModuleName = module.Fields.Where(q => q.Name == firstModule).FirstOrDefault().LookupType;

                                        fieldUpdateRecords.Add(secondModule, (int)record[secondModule + "id"]);
                                    }
                                }
                            }
                            else
                            {
                                if (fieldUpdate.Module.Name == module.Name)
                                {
                                    fieldUpdateRecords.Add(module.Name, (int)record["id"]);
                                }
                                else
                                {
                                    foreach (var field in module.Fields)
                                    {
                                        if (field.LookupType != null && field.LookupType != "users" && field.LookupType == fieldUpdate.Module.Name && !record[field.Name + "." + fieldUpdate.Field].IsNullOrEmpty())
                                        {
                                            fieldUpdateRecords.Add(fieldUpdate.Module.Name, (int)record[field.Name + ".id"]);
                                        }
                                    }
                                }
                            }

                            foreach (var fieldUpdateRecord in fieldUpdateRecords)
                            {
                                Module fieldUpdateModule;

                                if (fieldUpdateRecord.Key == module.Name)
                                    fieldUpdateModule = module;
                                else
                                    fieldUpdateModule = await moduleRepository.GetByName(fieldUpdateRecord.Key);

                                if (fieldUpdateModule == null)
                                {
                                    throw new Exception("Module not found! ModuleName: " + fieldUpdateModule.Name);
                                }

                                var currentRecordFieldUpdate = recordRepository.GetById(fieldUpdateModule, fieldUpdateRecord.Value, false);

                                if (currentRecordFieldUpdate == null)
                                {
                                    throw new Exception("Record not found! ModuleName: " + fieldUpdateModule.Name + " RecordId:" + fieldUpdateRecord.Value);
                                }

                                var recordFieldUpdate = new JObject();
                                recordFieldUpdate["id"] = currentRecordFieldUpdate["id"];

                                if (!isDynamicUpdate)
                                    recordFieldUpdate[fieldUpdate.Field.Name] = fieldUpdate.Value;
                                else
                                {
                                    if (type == 0 || type == 1 || type == 2)
                                    {
                                        bool isLookup = false;
                                        foreach (var field in module.Fields)
                                        {
                                            if (field.Name == fieldUpdate.Value && field.LookupType != null)
                                                isLookup = true;
                                        }

                                        if (isLookup)
                                            recordFieldUpdate[fieldUpdate.Field.Name] = record[fieldUpdate.Value + ".id"];
                                        else
                                        {
                                            bool isTag = false;
                                            foreach (var field in module.Fields)
                                            {
                                                if (field.Name == fieldUpdate.Value && field.DataType == DataType.Tag)
                                                    isTag = true;
                                            }

                                            if (isTag)
                                                recordFieldUpdate[fieldUpdate.Field.Name] = string.Join(",", record[fieldUpdate.Value]);
                                            else
                                                recordFieldUpdate[fieldUpdate.Field.Name] = record[fieldUpdate.Value];
                                        }
                                    }
                                    else if (type == 3 || type == 4)
                                        recordFieldUpdate[fieldUpdate.Field.Name] = record[firstModule + "." + fieldUpdate.Value];
                                }


                                var modelState = new ModelStateDictionary();
                                var resultBefore = await recordHelper.BeforeCreateUpdate(fieldUpdateModule, recordFieldUpdate, modelState, appUser.Language, moduleRepository, picklistRepository, profileRepository, tagRepository, settingRepository, false, currentRecordFieldUpdate, appUser: appUser);

                                if (resultBefore < 0 && !modelState.IsValid)
                                {
                                    throw new OperationCanceledException("Record cannot be updated! Object: " + recordFieldUpdate + " ModelState: " + modelState.ToJsonString());
                                }

                                if (isDynamicUpdate)
                                    recordRepository.MultiselectsToString(fieldUpdateModule, recordFieldUpdate);

                                try
                                {
                                    var resultUpdate = await recordRepository.Update(recordFieldUpdate, fieldUpdateModule);

                                    if (resultUpdate < 1)
                                    {
                                        throw new OperationCanceledException("Record cannot be updated! Object: " + recordFieldUpdate);
                                    }

                                    // If module is opportunities create stage history
                                    if (fieldUpdateModule.Name == "opportunities")
                                        await recordHelper.UpdateStageHistory(recordFieldUpdate, currentRecordFieldUpdate);
                                }
                                catch (Exception ex)
                                {
                                    ErrorHandler.LogError(ex, $"Record can't update" + " " + "tenant_id:" + currentUser.TenantId + "fieldUpdateModule:" + fieldUpdateModule + " recordFieldUpdate:" + recordFieldUpdate + " recordFieldUpdate:" + recordFieldUpdate + " currentRecordFieldUpdate:" + currentRecordFieldUpdate);
                                }

                                //TODO RecordHelper
                                recordHelper.AfterUpdate(fieldUpdateModule, recordFieldUpdate, currentRecordFieldUpdate, appUser, warehouse, fieldUpdateModule.Id != module.Id, false);
                            }

                            return ExecutionResult.Next();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, $"Data Update Step Error"); 
                return ExecutionResult.Next();
            }

        }
    }
}