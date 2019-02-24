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
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			if (context == null)
				throw new NullReferenceException();

			if (context.Workflow.Reference == null)
				throw new NullReferenceException();

			var appUser = JsonConvert.DeserializeObject<UserItem>(context.Workflow.Reference);
			var _currentUser = new CurrentUser { };

			if (!string.IsNullOrEmpty(previewMode))
			{
				_currentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };
			}
			var newRequest = JObject.Parse(Request.Replace("\\", ""));

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

				using (var _platformWarehouseRepository = new PlatformWarehouseRepository(platformDatabaseContext, _configuration, cacheHelper))
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
						_moduleRepository.CurrentUser = _recordRepository.CurrentUser = _userRepository.CurrentUser = _currentUser;

						var data = JObject.FromObject(context.Workflow.Data);

						if (newRequest["send_notification"].IsNullOrEmpty() || data["module_id"].IsNullOrEmpty())
							throw new MissingFieldException("Cannot find child data");

						var tempSendNotification = newRequest["send_notification"];
						var sendNotification = new BpmNotification
						{
							Subject = tempSendNotification["subject"].Value<string>(),
							Message = tempSendNotification["message"].Value<string>(),
							RecipientList = tempSendNotification["recipients"].ToObject<JArray>(),
							CC = !tempSendNotification["cc"].IsNullOrEmpty() ? tempSendNotification["cc"].ToObject<JArray>() : new JArray(),
							Bcc = !tempSendNotification["bcc"].IsNullOrEmpty() ? tempSendNotification["bcc"].ToObject<JArray>() : new JArray(),
							Schedule = tempSendNotification["schedule"]["value"].Value<string>() == "now" ? 0 : tempSendNotification["schedule"]["value"].ToObject<int>()
						};

						var recipients = sendNotification.RecipientList;

						var moduleId = data["module_id"].ToObject<int>();
						var module = await _moduleRepository.GetById(moduleId);

						//var recordId = data["record"].ToObject<int>();
						var record = data["record"].ToObject<JObject>();//_recordRepository.GetById(module, recordId);

						var sendNotificationCC = new JArray();//sendNotification.CC;
						var sendNotificationBCC = new JArray();//sendNotification.Bcc;

						if (sendNotification.CCArray != null && sendNotification.CCArray.Length == 1/* && !sendNotification.CC.Contains("@")*/)
							sendNotificationCC.Add(!record[sendNotification.CCArray[0]].IsNullOrEmpty() ? record[sendNotification.CCArray[0].ToString()].ToString() : null);
						else
							sendNotificationCC = sendNotification.CC;

						if (sendNotification.BccArray != null && sendNotification.BccArray.Length == 1 /* && !sendNotification.Bcc.Contains("@")*/)
							sendNotificationBCC.Add(!record[sendNotification.BccArray[0]].IsNullOrEmpty() ? record[sendNotification.BccArray[0].ToString()].ToString() : null);
						else
							sendNotificationBCC = sendNotification.Bcc;

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
						var testMode = _configuration.GetValue("AppSettings:TestMode", string.Empty);
						var subdomain = "";
						if (!string.IsNullOrEmpty(testMode))
						{
							subdomain = testMode == "true" ? "test" : appDomain;
						}
						domain = string.Format(domain, subdomain);
						//domain = "http://localhost:5554/";

						using (var _appRepository = new ApplicationRepository(platformDatabaseContext, _configuration, cacheHelper))
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
							if (recipient.HasValues)
							{
								if (recipient["email"].ToString() == "[owner]")
								{
									var recipentUser = await _userRepository.GetById((int)record["owner.id"]);

									if (recipentUser == null)
										continue;

									recipient["email"] = recipentUser.Email;

								}
								if (!recipient["email"].ToString().Contains("@"))
								{
									if (!record[recipient].IsNullOrEmpty())
									{
										recipient["email"] = (string)record[recipient["email"].ToString()];
									}
								}
								email.AddRecipient(recipient["email"].ToString());
							}

							//if (recipients.Contains(recipient))
							//    continue;

							if (!recipient.HasValues)
							{
								if (!record[recipient.ToString()].IsNullOrEmpty())
								{
									recipient = (string)record[recipient.ToString()];
								}
								email.AddRecipient(recipient.ToString());
							}

						}

						if (sendNotification.Schedule.HasValue)
							email.SendOn = DateTime.UtcNow.AddDays(sendNotification.Schedule.Value);

						email.AddToQueue(appUser.TenantId, module.Id, (int)record["id"], "", "", sendNotificationCC == null ? "" : string.Join(",", sendNotificationCC.ToObject<string[]>()), sendNotificationBCC == null ? "" : string.Join(",", sendNotificationBCC.ToObject<string[]>()), appUser: appUser, addRecordSummary: false);

						//var workflowLog = new BpmWorkflowLog
						//{
						//    WorkflowId = workflow.Id,
						//    ModuleId = module.Id,
						//    RecordId = (int)record["id"]
						//};
					}
				}
			}
			ExecutionResult.Outcome(Response).OutcomeValue = Response;
			return ExecutionResult.Next();
		}
	}
}
