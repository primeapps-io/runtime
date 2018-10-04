using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Resources;
using System.Linq;

namespace PrimeApps.App.Bpm.Steps
{
    public class NotificationStep : StepBodyAsync
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;

        public string Request { get; set; }
        public string Response { get; set; }

        public NotificationStep(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
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

            var appUser = JsonConvert.DeserializeObject<UserItem>(context.Workflow.Reference);
            var _currentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id, };

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
                    using (var _userRepository = new UserRepository(databaseContext, _configuration))
                    {
                        _moduleRepository.CurrentUser = _currentUser;

                        if (newRequest["FieldUpdate"].IsNullOrEmpty() || newRequest["Module"].IsNullOrEmpty())
                            throw new MissingFieldException("Cannot find child data");

                        var sendNotification = newRequest["SendNotification"].ToObject<BpmNotification>();
                        var module = newRequest["Module"].ToObject<Module>(); //module ID olarak gelirse module bilgisi çekilecek.
                        var record = newRequest["record"];

                        if (newRequest["FieldUpdate"].HasValues)
                        {
                            var lookupModuleNames = new List<string>();
                            ICollection<Module> lookupModules = null;

                            foreach (var field in module.Fields)
                            {
                                if (!field.Deleted && field.DataType == DataType.Lookup && field.LookupType != "users" && field.LookupType != "relation" && !lookupModuleNames.Contains(field.LookupType))
                                    lookupModuleNames.Add(field.LookupType);
                            }

                            if (lookupModuleNames.Count > 0)
                                lookupModules = await _moduleRepository.GetByNamesBasic(lookupModuleNames);
                            else
                                lookupModules = new List<Module>();

                            lookupModules.Add(Model.Helpers.ModuleHelper.GetFakeUserModule());
                            record = _recordRepository.GetById(module, (int)record["id"], false, lookupModules, true);
                        }

                        var recipients = sendNotification.RecipientsArray;
                        var sendNotificationCC = sendNotification.CC;
                        var sendNotificationBCC = sendNotification.Bcc;

                        if (sendNotification.CCArray != null && sendNotification.CCArray.Length == 1 && !sendNotification.CC.Contains("@"))
                            sendNotificationCC = (string)record[sendNotification.CC];

                        if (sendNotification.BccArray != null && sendNotification.BccArray.Length == 1 && !sendNotification.Bcc.Contains("@"))
                            sendNotificationBCC = (string)record[sendNotification.Bcc];

                        string domain;

                        domain = "https://{0}.ofisim.com/";
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

                        var subdomain = _configuration.GetSection("TestMode")["BlobUrl"] == "true" ? "test" : appDomain;

                        domain = string.Format(domain, subdomain);
                        //domain = "http://localhost:5554/";

                        using (var _appRepository = new ApplicationRepository(platformDatabaseContext, _configuration))
                        {
                            var app = await _appRepository.Get(appUser.AppId);
                            if (app != null)
                                domain = "https://" + app.Setting.AppDomain + "/";
                        }

                        var url = domain + "#/app/module/" + module.Name + "?id=" + record["id"];

                        var emailData = new Dictionary<string, string>();
                        emailData.Add("Subject", sendNotification.Subject);
                        emailData.Add("Content", sendNotification.Message);
                        emailData.Add("Url", url);

                        var email = new Email(EmailResource.WorkflowNotification, appUser.Culture, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);

                        foreach (var recipientItem in recipients)
                        {
                            var recipient = recipientItem;

                            if (recipient == "[owner]")
                            {
                                var recipentUser = await _userRepository.GetById((int)record["owner.id"]);

                                if (recipentUser == null)
                                    continue;

                                recipient = recipentUser.Email;

                            }

                            if (recipients.Contains(recipient))
                                continue;

                            if (!recipient.Contains("@"))
                            {
                                if (!record[recipient].IsNullOrEmpty())
                                {
                                    recipient = (string)record[recipient];
                                }
                            }
                            email.AddRecipient(recipient);
                        }

                        if (sendNotification.Schedule.HasValue)
                            email.SendOn = DateTime.UtcNow.AddDays(sendNotification.Schedule.Value);

                        email.AddToQueue(appUser.TenantId, module.Id, (int)record["id"], "", "", sendNotificationCC, sendNotificationBCC, appUser: appUser, addRecordSummary: false);

                        //var workflowLog = new BpmWorkflowLog
                        //{
                        //    WorkflowId = workflow.Id,
                        //    ModuleId = module.Id,
                        //    RecordId = (int)record["id"]
                        //};
                    }
                }
            }

            return ExecutionResult.Next();
        }
    }
}
