using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Steps
{
    public class DataUpdateStep : StepBody
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;

        public JObject Request { get; set; }
        public JObject Response { get; set; }

        public DataUpdateStep(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            if (context == null)
                throw new NullReferenceException();

            if (context.Workflow.Reference == null)
                throw new NullReferenceException();

            var appUser = JsonConvert.DeserializeObject<JObject>(context.Workflow.Reference);

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                
                    var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var analyticRepository = new AnalyticRepository(databaseContext, _configuration)) {
                    //warehouse gelicek.
                    using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
                    {
                        if (Request["FieldUpdate"].IsNullOrEmpty() || Request["Module"].IsNullOrEmpty())
                            throw new MissingFieldException("Cannot find child data");

                        var fieldUpdate = Request["FieldUpdate"].ToObject<BpmDataUpdate>();
                        var module = Request["Module"].ToObject<Module>(); //module ID olarak gelirse module bilgisi çekilecek.
                        var record = Request["record"];

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
                            
                        }

                        return ExecutionResult.Next();
                    }
                }
            }
        }
    }
}
