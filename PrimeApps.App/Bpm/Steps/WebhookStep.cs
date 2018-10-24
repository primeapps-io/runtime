using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Context;
using Newtonsoft.Json;
using PrimeApps.Model.Common.Cache;
using System.Linq;
using System.Web;
using System.Net.Http.Headers;

namespace PrimeApps.App.Bpm.Steps
{
    public class WebhookStep : StepBodyAsync
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;

        public string Request { get; set; }
        public string Response { get; set; }

        public WebhookStep(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
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

            //TODO REf Kontrol
            var appUser = JsonConvert.DeserializeObject<UserItem>(context.Workflow.Reference);
            var _currentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };

            var newRequest = Request != null ? JObject.Parse(Request.Replace("\\", "")) : null;

            if (newRequest.IsNullOrEmpty())
                throw new DataMisalignedException("Cannot find Request");

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
                    using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
                    using (var client = new HttpClient())
                    {
                        _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _picklistRepository.CurrentUser = _currentUser;

                        var data = JObject.FromObject(context.Workflow.Data);

                        var record= data["record"].ToObject<JObject>();

                        var moduleId = data["module_id"].ToObject<int>();
                        var module = await _moduleRepository.GetById(moduleId);
                        
                        var webHook = newRequest["webHook"];

                        if (!webHook.HasValues)
                            throw new MissingFieldException("Cannot find child data");

                        var jsonData = new JObject();
                        jsonData["id"] = record["id"];
                        var recordId = record["id"].ToObject<int>();
                        
                        if (!webHook["Parameters"].IsNullOrEmpty())
                        {
                            var parameters = webHook["Parameters"];
                            var lookModuleNames = new List<string>();
                            ICollection<Module> lookupModules = null;

                            foreach (var field in module.Fields)
                            {
                                if (!field.Deleted && field.DataType == DataType.Lookup && field.LookupType != "users" && field.LookupType != "relation" && !lookModuleNames.Contains(field.LookupType))
                                    lookModuleNames.Add(field.LookupType);
                            }

                            if (lookModuleNames.Count > 0)
                                lookupModules = await _moduleRepository.GetByNamesBasic(lookModuleNames);
                            else
                                lookupModules = new List<Module>();

                            lookupModules.Add(ModuleHelper.GetFakeUserModule());

                            var recordData = _recordRepository.GetById(module, recordId, false, lookupModules, true);
                            recordData = await RecordHelper.FormatRecordValues(module, recordData, _moduleRepository, _picklistRepository, _configuration, appUser.TenantGuid, appUser.Language, appUser.Culture, 180, lookupModules);

                            foreach (var dataString in parameters)
                            {
                                //var dataObject = dataString.Split('|');
                                var parameterName = dataString["parameterName"].Value<string>();
                                var moduleName = dataString["selectedModule"]["name"].Value<string>();
                                var fieldName = dataString["selectedField"]["name"].Value<string>();

                                if (moduleName != module.Name)
                                    jsonData[parameterName] = recordData[moduleName + "." + fieldName];
                                else
                                {
                                    var currentField = module.Fields.Single(q => q.Name == fieldName);

                                    if (currentField.DataType == DataType.Lookup && currentField.LookupType == "users")
                                        jsonData[parameterName] = recordData[fieldName + ".full_name"];
                                    else
                                        jsonData[parameterName] = recordData[fieldName];

                                }
                            }
                        }

                        var methodType = webHook["methodType"].ToObject<BpmHttpMethod>();
                        switch (methodType)
                        {
                            case BpmHttpMethod.Post:
                                if (jsonData.HasValues)
                                {
                                    //fire and forget
                                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                                    var dataAsString = JsonConvert.SerializeObject(jsonData);
                                    var contentResetPasswordBindingModel = new StringContent(dataAsString);
                                    await client.PostAsync(webHook["callbackUrl"].Value<string>(), contentResetPasswordBindingModel);

                                    //await client.PostAsJsonAsync(webHook.CallbackUrl, jsonData);
                                }
                                else
                                {
                                    await client.PostAsync(webHook["callbackUrl"].Value<string>(), null);
                                }

                                break;
                            case BpmHttpMethod.Get:
                                if (jsonData.HasValues)
                                {
                                    var query = String.Join("&",
                                        jsonData.Children().Cast<JProperty>()
                                        .Select(q => q.Name + "=" + HttpUtility.UrlEncode(q.Value.ToString())));

                                    await client.GetAsync(webHook["callbackUrl"].Value<string>() + "?" + query);
                                }
                                else
                                {
                                    await client.GetAsync(webHook["callbackUrl"].Value<string>());
                                }
                                break;
                        }
                    }
                }
            }
            return ExecutionResult.Next();
        }
    }
}
