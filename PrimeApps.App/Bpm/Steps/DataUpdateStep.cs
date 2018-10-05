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
            if (context == null)
                throw new NullReferenceException();

            if (context.Workflow.Reference == null)
                throw new NullReferenceException();

            //var tempRef = context.Workflow.Reference.Split('|');
            //var _currentUser = new CurrentUser { TenantId = int.Parse(tempRef[0]), UserId = int.Parse(tempRef[1]) };
            //var tenantLanguage = tempRef[2];

            var appUser = JsonConvert.DeserializeObject<Reference>(context.Workflow.Reference);
            var _currentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };

            var newRequest = JObject.Parse(Request.Replace("\\", ""));

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();

                using (var _platformWarehouseRepository = new PlatformWarehouseRepository(platformDatabaseContext, _configuration))
                using (var _analyticRepository = new AnalyticRepository(databaseContext, _configuration))
                {
                    _platformWarehouseRepository.CurrentUser = _analyticRepository.CurrentUser = _currentUser;

                    var warehouse = new Model.Helpers.Warehouse(_analyticRepository, _configuration);
                    var warehouseEntity = await _platformWarehouseRepository.GetByTenantId(_currentUser.TenantId);

                    if (warehouseEntity != null)
                        warehouse.DatabaseName = warehouseEntity.DatabaseName;
                    else
                        warehouse.DatabaseName = "0";

                    using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                    using (var _recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
                    using (var _recordHelper = new RecordHelper(_configuration, _serviceScopeFactory, _currentUser))
                    {
                        _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;

                        if (newRequest["FieldUpdate"].IsNullOrEmpty() || newRequest["Module"].IsNullOrEmpty())
                            throw new MissingFieldException("Cannot find child data");

                        var fieldUpdate = newRequest["FieldUpdate"].ToObject<BpmDataUpdate>();
                        var moduleID = newRequest["Module"].ToObject<int>(); 
                        var module = await _moduleRepository.GetById(moduleID);
                        var record = newRequest["record"];

                        var fieldUpdateRecords = new Dictionary<string, int>();
                        var isDynamicUpdate = false;
                        var type = 0;
                        var firstModule = string.Empty;
                        var secondModule = string.Empty;

                        if (fieldUpdate.Module.Contains(','))
                        {
                            isDynamicUpdate = true;
                            var modules = fieldUpdate.Module.Split(',');
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
                            if (fieldUpdate.Module == module.Name)
                            {
                                fieldUpdateRecords.Add(module.Name, (int)record["id"]);
                            }
                            else
                            {
                                foreach (var field in module.Fields)
                                {
                                    if (field.LookupType != null && field.LookupType != "users" && field.LookupType == fieldUpdate.Module && !record[field.Name + "." + fieldUpdate.Field].IsNullOrEmpty())
                                    {
                                        fieldUpdateRecords.Add(fieldUpdate.Module, (int)record[field.Name + ".id"]);
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
                                fieldUpdateModule = await _moduleRepository.GetByName(fieldUpdateRecord.Key);

                            if (fieldUpdateModule == null)
                            {
                                throw new DataMisalignedException("Module not found! ModuleName: " + fieldUpdateModule.Name);
                            }

                            var currentRecordFieldUpdate = _recordRepository.GetById(fieldUpdateModule, fieldUpdateRecord.Value, false);

                            if (currentRecordFieldUpdate == null)
                            {
                                throw new DataMisalignedException("Record not found! ModuleName: " + fieldUpdateModule.Name + " RecordId:" + fieldUpdateRecord.Value);
                            }

                            var recordFieldUpdate = new JObject();
                            recordFieldUpdate["id"] = currentRecordFieldUpdate["id"];

                            if (!isDynamicUpdate)
                                recordFieldUpdate[fieldUpdate.Field] = fieldUpdate.Value;
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
                                        recordFieldUpdate[fieldUpdate.Field] = record[fieldUpdate.Value + ".id"];
                                    else
                                    {
                                        bool isTag = false;
                                        foreach (var field in module.Fields)
                                        {
                                            if (field.Name == fieldUpdate.Value && field.DataType == DataType.Tag)
                                                isTag = true;
                                        }

                                        if (isTag)
                                            recordFieldUpdate[fieldUpdate.Field] = string.Join(",", record[fieldUpdate.Value]);
                                        else
                                            recordFieldUpdate[fieldUpdate.Field] = record[fieldUpdate.Value];
                                    }
                                }
                                else if (type == 3 || type == 4)
                                    recordFieldUpdate[fieldUpdate.Field] = record[firstModule + "." + fieldUpdate.Value];
                            }



                            var modelState = new ModelStateDictionary();
                            var resultBefore = await _recordHelper.BeforeCreateUpdate(fieldUpdateModule, recordFieldUpdate, modelState, appUser.Language, false, currentRecordFieldUpdate, null);

                            if (resultBefore < 0 && !modelState.IsValid)
                            {
                                throw new OperationCanceledException("Record cannot be updated! Object: " + recordFieldUpdate + " ModelState: " + modelState.ToJsonString());
                            }

                            if (isDynamicUpdate)
                                _recordRepository.MultiselectsToString(fieldUpdateModule, recordFieldUpdate);

                            try
                            {
                                var resultUpdate = await _recordRepository.Update(recordFieldUpdate, fieldUpdateModule);

                                if (resultUpdate < 1)
                                {
                                    throw new OperationCanceledException("Record cannot be updated! Object: " + recordFieldUpdate);
                                }

                                // If module is opportunities create stage history
                                if (fieldUpdateModule.Name == "opportunities")
                                    await _recordHelper.UpdateStageHistory(recordFieldUpdate, currentRecordFieldUpdate);

                            }
                            catch (Exception ex)
                            {
                                //ErrorLog.GetDefault(null).Log(new Error(ex));
                            }

                            //TODO RecordHelper
                            //AfterUpdate(fieldUpdateModule, recordFieldUpdate, currentRecordFieldUpdate, appUser, warehouse, fieldUpdateModule.Id != module.Id, false);
                        }

                        return ExecutionResult.Next();
                    }
                }
            }
        }
    }
}
