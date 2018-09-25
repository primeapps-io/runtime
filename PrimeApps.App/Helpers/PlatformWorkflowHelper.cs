using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Models;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using System.Web;
using System.Configuration;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.IdentityModel.Protocols;
using PrimeApps.Model.Common.Cache;
using Newtonsoft.Json;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Common.Resources;
using Application = PrimeApps.Model.Entities.Platform.App;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PrimeApps.App.Helpers
{
	public interface IPlatformWorkflowHelper
	{
		Task Run(OperationType operationType, Application application);
		AppWorkflow CreateEntity(PlatformWorkflowBindingModels workflowModel);
		void UpdateEntity(PlatformWorkflowBindingModels workflowModel, AppWorkflow workflow);
	}
	public class PlatformWorkflowHelper : IPlatformWorkflowHelper
	{
		private IConfiguration _configuration;
		private IServiceScopeFactory _serviceScopeFactory;

		public PlatformWorkflowHelper(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
		{
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}
		public async Task Run(OperationType operationType, Application application)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				using (var _platformWorkflowRepository = new PlatformWorkflowRepository(databaseContext, _configuration))
				{
					var workflows = await _platformWorkflowRepository.GetAll(application.Id, true);
					workflows = workflows.Where(x => x.OperationsArray.Contains(operationType.ToString())).ToList();

					if (workflows.Count < 1)
						return;

					foreach (var workflow in workflows)
					{
						if (workflow.WebHook != null)
						{
							try
							{
								var webHook = workflow.WebHook;

								using (var client = new HttpClient())
								{
									var jsonData = new JObject();
									jsonData["message"] = "Workflow deneme mesajıdır.";

									switch (webHook.MethodType)
									{
										case WorkflowHttpMethod.Post:
											if (jsonData.HasValues)
											{
												//fire and forget
												client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

												var dataAsString = JsonConvert.SerializeObject(jsonData);
												var contentResetPasswordBindingModel = new StringContent(dataAsString);
												await client.PostAsync(webHook.CallbackUrl, contentResetPasswordBindingModel);
											}
											else
											{
												await client.PostAsync(webHook.CallbackUrl, null);
											}

											break;
										case WorkflowHttpMethod.Get:
											if (jsonData.HasValues)
											{
												var query = String.Join("&",
													jsonData.Children().Cast<JProperty>()
													.Select(jp => jp.Name + "=" + HttpUtility.UrlEncode(jp.Value.ToString())));

												await client.GetAsync(webHook.CallbackUrl + "?" + query);
											}
											else
											{
												await client.GetAsync(webHook.CallbackUrl);
											}
											break;
									}
								}
							}
							catch (Exception ex)
							{
								//ErrorLog.GetDefault(null).Log(new Error(ex));
							}
						}

						var workflowLog = new AppWorkflowLog
						{
							AppWorkflowId = workflow.Id,
							AppId = application.Id
						};

						try
						{
							var resultCreateLog = await _platformWorkflowRepository.CreateLog(workflowLog);

							//if (resultCreateLog < 1)
							//    ErrorLog.GetDefault(null).Log(new Error(new Exception("WorkflowLog cannot be created! Object: " + workflowLog.ToJsonString())));
						}
						catch (Exception ex)
						{
							//ErrorLog.GetDefault(null).Log(new Error(ex));
						}
					}
				}
			}
			
		}

		public AppWorkflow CreateEntity(PlatformWorkflowBindingModels workflowModel)
		{
			var workflow = new AppWorkflow
			{
				AppId = workflowModel.AppId,
				Name = workflowModel.Name,
				Frequency = workflowModel.Frequency,
				Active = workflowModel.Active,
				OperationsArray = workflowModel.Operations
			};

			if (workflowModel.Actions.WebHook != null)
			{
				workflow.WebHook = new AppWorkflowWebhook
				{
					CallbackUrl = workflowModel.Actions.WebHook.CallbackUrl,
					MethodType = workflowModel.Actions.WebHook.MethodType,
					Parameters = workflowModel.Actions.WebHook.Parameters
				};
			}

			return workflow;
		}

		public void UpdateEntity(PlatformWorkflowBindingModels workflowModel, AppWorkflow workflow)
		{
			workflow.Name = workflowModel.Name;
			workflow.Frequency = workflowModel.Frequency;
			workflow.Active = workflowModel.Active;
			workflow.OperationsArray = workflowModel.Operations;

			if (workflowModel.Actions.WebHook != null)
			{
				if (workflow.WebHook == null)
					workflow.WebHook = new AppWorkflowWebhook();

				workflow.WebHook.CallbackUrl = workflowModel.Actions.WebHook.CallbackUrl;
				workflow.WebHook.MethodType = workflowModel.Actions.WebHook.MethodType;
				workflow.WebHook.Parameters = workflowModel.Actions.WebHook.Parameters;

			}
			else
			{
				workflow.WebHook = null;
			}
		}
	}
}
