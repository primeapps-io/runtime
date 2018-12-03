using Humanizer.Localisation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Resources;
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
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.App.Helpers
{

	public interface IWorkflowHelper
	{
		Task Run(OperationType operationType, JObject record, Module module, UserItem appUser, Warehouse warehouse, BeforeCreateUpdate BeforeCreateUpdate, UpdateStageHistory UpdateStageHistory, AfterUpdate AfterUpdate, AfterCreate AfterCreate, JObject previousRecord = null);

		Task<Workflow> CreateEntity(WorkflowBindingModel workflowModel, string tenantLanguage);

		Task UpdateEntity(WorkflowBindingModel workflowModel, Workflow workflow, string tenantLanguage);
	}

	public class WorkflowHelper : IWorkflowHelper
	{
		private CurrentUser _currentUser;
		private IHttpContextAccessor _context;
		private IConfiguration _configuration;
		private IServiceScopeFactory _serviceScopeFactory;

		public WorkflowHelper(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor context, IConfiguration configuration)
		{
			_context = context;
			_currentUser = UserHelper.GetCurrentUser(_context);
			_serviceScopeFactory = serviceScopeFactory;
			_configuration = configuration;
		}

		public WorkflowHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, CurrentUser currentUser)
		{
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;

			_currentUser = currentUser;

		}

		public async Task Run(OperationType operationType, JObject record, Module module, UserItem appUser, Warehouse warehouse, BeforeCreateUpdate BeforeCreateUpdate, UpdateStageHistory UpdateStageHistory, AfterUpdate AfterUpdate, AfterCreate AfterCreate, JObject previousRecord)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				//Set warehouse database name
				warehouse.DatabaseName = appUser.WarehouseDatabaseName;

				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				var platformDatabaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				using (var _workflowRepository = new WorkflowRepository(databaseContext, _configuration))
				using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
				using (var _recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
				using (var _profileRepository = new ProfileRepository(databaseContext, _configuration))
				using (var _userRepository = new UserRepository(databaseContext, _configuration))
				using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
				using (var _settingRepository = new SettingRepository(databaseContext, _configuration))
				{
					_profileRepository.CurrentUser = _userRepository.CurrentUser = _picklistRepository.CurrentUser = _workflowRepository.CurrentUser = _moduleRepository.CurrentUser = _recordRepository.CurrentUser = _settingRepository.CurrentUser = _currentUser;
					var workflows = await _workflowRepository.GetAll(module.Id, true);
					workflows = workflows.Where(x => x.OperationsArray.Contains(operationType.ToString())).ToList();
					var culture = CultureInfo.CreateSpecificCulture(appUser.Culture);

					if (workflows.Count < 1)
						return;

					var lookupModuleNames = new List<string>();
					ICollection<Module> lookupModules = null;

					foreach (var workflow in workflows)
					{
						if (!string.IsNullOrEmpty(workflow.ChangedField) && operationType != OperationType.insert)
						{
							bool isFieldExist = false;
							var relatedField = module.Fields.FirstOrDefault(x => x.Name == workflow.ChangedField);

							foreach (var property in record)
							{
								if (property.Key == workflow.ChangedField || property.Key == workflow.ChangedField + ".id")
									isFieldExist = true;
							}

							if (!isFieldExist)
							{
								continue;
							}
							else
							{
								if (relatedField.DataType == DataType.Lookup)
								{
									var hasId = false;
									foreach (var property in record)
									{
										if (property.Key == workflow.ChangedField + ".id")
											hasId = true;
									}

									if (hasId)
									{
										if (!record[workflow.ChangedField + ".id"].IsNullOrEmpty() && !previousRecord[workflow.ChangedField].IsNullOrEmpty())
										{
											if (((int)record[workflow.ChangedField + ".id"] == (int)previousRecord[workflow.ChangedField]))
												continue;

										}
										else if (record[workflow.ChangedField + ".id"].IsNullOrEmpty() && previousRecord[workflow.ChangedField].IsNullOrEmpty())
										{
											continue;
										}
									}
									else
									{
										if (!record[workflow.ChangedField].IsNullOrEmpty() && !previousRecord[workflow.ChangedField].IsNullOrEmpty())
										{
											if (((int)record[workflow.ChangedField] == (int)previousRecord[workflow.ChangedField]))
												continue;
										}
										else if (record[workflow.ChangedField].IsNullOrEmpty() && previousRecord[workflow.ChangedField].IsNullOrEmpty())
										{
											continue;
										}

									}

								}
								else
								{
									if ((string)record[workflow.ChangedField] == (string)previousRecord[workflow.ChangedField])
										continue;
								}
							}

						}

						lookupModuleNames = new List<string>();
						lookupModules = null;

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

						bool hasProcessFilter = true;
						if (workflow.ProcessFilter != WorkflowProcessFilter.None)
						{
							if (!record["process_id"].IsNullOrEmpty() || (record["process_id"].IsNullOrEmpty() && workflow.ProcessFilter == WorkflowProcessFilter.All))
							{
								switch (workflow.ProcessFilter)
								{
									case WorkflowProcessFilter.All:
										break;
									case WorkflowProcessFilter.Pending:
										if ((int)record["process_status"] != 1)
											hasProcessFilter = false;
										break;
									case WorkflowProcessFilter.Approved:
										if ((int)record["process_status"] != 2)
											hasProcessFilter = false;
										break;
									case WorkflowProcessFilter.Rejected:
										if ((int)record["process_status"] != 3)
											hasProcessFilter = false;
										break;
								}
							}
							else continue;
						}

						if (!hasProcessFilter)
							continue;

						if ((workflow.Frequency == WorkflowFrequency.NotSet || workflow.Frequency == WorkflowFrequency.OneTime) && operationType != OperationType.delete)
						{
							var hasLog = await _workflowRepository.HasLog(workflow.Id, module.Id, (int)record["id"]);

							if (hasLog)
								continue;
						}

						if (workflow.Filters != null && workflow.Filters.Count > 0)
						{
							var filters = workflow.Filters;
							var mismatchedCount = 0;

							foreach (var filter in filters)
							{
								var filterField = module.Fields.FirstOrDefault(x => x.Name == filter.Field);
								var filterFieldStr = filter.Field;

								if (filterField.DataType == DataType.Lookup && !filter.Field.EndsWith(".id"))
									filterFieldStr = filter.Field + ".id";

								if (filterField == null || record[filterFieldStr] == null)
								{
									mismatchedCount++;
									continue;
								}

								var filterOperator = filter.Operator;
								var fieldValueString = record[filterFieldStr].ToString();
								var filterValueString = filter.Value;
								double fieldValueNumber;
								double filterValueNumber;
								double.TryParse(fieldValueString, out fieldValueNumber);
								double.TryParse(filterValueString, out filterValueNumber);
								bool fieldValueBoolen;
								bool filterValueBoolen;
								var fieldValueBoolenParsed = bool.TryParse(fieldValueString, out fieldValueBoolen);
								var filterValueBoolenParsed = bool.TryParse(filterValueString, out filterValueBoolen);

								if (filterField.DataType == DataType.Lookup && filterField.LookupType == "users" && filterValueNumber == 0)
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
										if (fieldValueBoolenParsed && filterValueBoolenParsed)
										{
											if (fieldValueBoolen != filterValueBoolen)
												mismatchedCount++;
										}
										else
										{
											if (fieldValueNumber != filterValueNumber)
												mismatchedCount++;
										}
										break;
									case Operator.NotEqual:
										if (fieldValueBoolenParsed && filterValueBoolenParsed)
										{
											if (fieldValueBoolen == filterValueBoolen)
												mismatchedCount++;
										}
										else
										{
											if (fieldValueNumber == filterValueNumber)
												mismatchedCount++;
										}
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
										if (filterField.DataType == DataType.Date)
										{
											if (!record[filter.Field].IsNullOrEmpty())
											{
												DateTime filterDate = Convert.ToDateTime(filter.Value);
												DateTime recordDate = Convert.ToDateTime(record[filter.Field]);

												if (recordDate <= filterDate)
												{
													mismatchedCount++;
												}
											}
											else
											{
												mismatchedCount++;
											}
										}
										else if (fieldValueNumber <= filterValueNumber)
										{
											mismatchedCount++;
										}
										break;
									case Operator.GreaterEqual:
										if (filterField.DataType == DataType.Date)
										{
											if (!record[filter.Field].IsNullOrEmpty())
											{
												DateTime filterDate = Convert.ToDateTime(filter.Value);
												DateTime recordDate = Convert.ToDateTime(record[filter.Field]);

												if (recordDate < filterDate)
												{
													mismatchedCount++;
												}
											}
											else
											{
												mismatchedCount++;
											}
										}
										else if (fieldValueNumber < filterValueNumber)
										{
											mismatchedCount++;
										}

										break;
									case Operator.Less:
										if (filterField.DataType == DataType.Date)
										{
											if (!record[filter.Field].IsNullOrEmpty())
											{
												DateTime filterDate = Convert.ToDateTime(filter.Value);
												DateTime recordDate = Convert.ToDateTime(record[filter.Field]);

												if (recordDate >= filterDate)
												{
													mismatchedCount++;
												}
											}
											else
											{
												mismatchedCount++;
											}
										}
										else if (fieldValueNumber >= filterValueNumber)
										{
											mismatchedCount++;
										}
										break;
									case Operator.LessEqual:
										if (filterField.DataType == DataType.Date)
										{
											if (!record[filter.Field].IsNullOrEmpty())
											{
												DateTime filterDate = Convert.ToDateTime(filter.Value);
												DateTime recordDate = Convert.ToDateTime(record[filter.Field]);

												if (recordDate >= filterDate)
												{
													mismatchedCount++;
												}
											}
											else
											{
												mismatchedCount++;
											}
										}
										else if (fieldValueNumber > filterValueNumber)
										{
											mismatchedCount++;
										}

										break;
								}
							}

							if (mismatchedCount > 0)
								continue;
						}

						if (workflow.FieldUpdate != null)
						{
							var fieldUpdate = workflow.FieldUpdate;
							var fieldUpdateRecords = new Dictionary<string, int>();
							var isDynamicUpdate = false;
							var type = 0;
							var firstModule = "";
							var secondModule = "";

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
										var secondModuleName = module.Fields.Where(x => x.Name == secondModule).FirstOrDefault().LookupType;

										foreach (var field in module.Fields)
										{
											if (field.LookupType != null && field.LookupType == secondModuleName && (!record[fieldUpdate.Value].IsNullOrEmpty() || !record[fieldUpdate.Value + ".id"].IsNullOrEmpty()))
											{
												if (fieldUpdateRecords.Count < 1)
													fieldUpdateRecords.Add(secondModuleName, (int)record[field.Name + ".id"]);
											}
										}
									}
									else if (firstModule != module.Name && secondModule == module.Name)
									{
										type = 3;
										var firstModuleName = module.Fields.Where(x => x.Name == firstModule).FirstOrDefault().LookupType;

										foreach (var field in module.Fields)
										{
											if (field.LookupType != null && field.LookupType == firstModuleName && (!record[fieldUpdate.Value].IsNullOrEmpty() || !record[firstModule + "." + fieldUpdate.Value].IsNullOrEmpty()))
											{
												if (fieldUpdateRecords.Count < 1)
													fieldUpdateRecords.Add(secondModule, (int)record["id"]);
											}
										}
									}
									else if (firstModule != module.Name && secondModule != module.Name)
									{
										type = 4;
										var firstModuleName = module.Fields.Where(x => x.Name == firstModule).FirstOrDefault().LookupType;
										var secondModuleName = module.Fields.Where(x => x.Name == firstModule).FirstOrDefault().LookupType;

										fieldUpdateRecords.Add(secondModuleName, (int)record[secondModule + ".id"]);
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
										if (field.LookupType != null && field.LookupType != "users" && field.LookupType == fieldUpdate.Module)
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
									ErrorHandler.LogError(new Exception("Module not found! ModuleName: " + fieldUpdateModule.Name), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
									return;
								}

								var currentRecordFieldUpdate = _recordRepository.GetById(fieldUpdateModule, fieldUpdateRecord.Value, false);

								if (currentRecordFieldUpdate == null)
								{
									ErrorHandler.LogError(new Exception("Record not found! ModuleName: " + fieldUpdateModule.Name + " RecordId:" + fieldUpdateRecord.Value), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
									return;
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
								var resultBefore = await BeforeCreateUpdate(fieldUpdateModule, recordFieldUpdate, modelState, appUser.TenantLanguage, _moduleRepository, _picklistRepository, _profileRepository, false, currentRecordFieldUpdate, appUser: appUser);

								if (resultBefore < 0 && !modelState.IsValid)
								{
									ErrorHandler.LogError(new Exception("Record cannot be updated! Object: " + recordFieldUpdate + " ModelState: " + modelState.ToJsonString()), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
									return;
								}

								if (isDynamicUpdate)
									_recordRepository.MultiselectsToString(fieldUpdateModule, recordFieldUpdate);

								try
								{

									var resultUpdate = await _recordRepository.Update(recordFieldUpdate, fieldUpdateModule);

									if (resultUpdate < 1)
									{
										ErrorHandler.LogError(new Exception("Record cannot be updated! Object: " + recordFieldUpdate), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
										return;
									}

									// If module is opportunities create stage history
									if (fieldUpdateModule.Name == "opportunities")
										await UpdateStageHistory(recordFieldUpdate, currentRecordFieldUpdate);
								}
								catch (Exception ex)
								{
									ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
								}

								AfterUpdate(fieldUpdateModule, recordFieldUpdate, currentRecordFieldUpdate, appUser, warehouse, fieldUpdateModule.Id != module.Id, false);
							}
						}

						if (workflow.CreateTask != null)
						{
							var moduleActivity = await _moduleRepository.GetByName("activities");

							var task = new JObject();
							task["activity_type"] = "1";
							task["owner"] = workflow.CreateTask.Owner;
							task["subject"] = workflow.CreateTask.Subject;

							if (module.Name != "activities")
							{
								task["related_module"] = 900000 + module.Id;
								task["related_to"] = (int)record["id"];
							}
							else if (!record["related_module"].IsNullOrEmpty())
							{
								var moduleCurrent = await _moduleRepository.GetByLabel((string)record["related_module"]);

								if (moduleCurrent == null) return;

								task["related_module"] = 900000 + moduleCurrent.Id;
								task["related_to"] = record["related_to"];
							}

							task["task_due_date"] = DateTime.UtcNow.AddDays(workflow.CreateTask.TaskDueDate);

							if (workflow.CreateTask.TaskStatus.HasValue)
								task["task_status"] = workflow.CreateTask.TaskStatus;

							if (workflow.CreateTask.TaskPriority.HasValue)
								task["task_priority"] = workflow.CreateTask.TaskPriority;

							if (workflow.CreateTask.TaskNotification.HasValue)
								task["task_notification"] = workflow.CreateTask.TaskNotification;

							if (workflow.CreateTask.TaskReminder.HasValue)
								task["task_reminder"] = workflow.CreateTask.TaskReminder;

							if (workflow.CreateTask.ReminderRecurrence.HasValue)
								task["reminder_recurrence"] = workflow.CreateTask.ReminderRecurrence;

							if (!string.IsNullOrWhiteSpace(workflow.CreateTask.Description))
								task["description"] = workflow.CreateTask.Description;

							if (workflow.CreateTask.Owner == 0)
								task["owner"] = !record["owner"].IsNullOrEmpty() ? (int)record["owner"] : (int)record["owner.id"];

							task["created_by"] = workflow.CreatedById;

							var modelState = new ModelStateDictionary();
							var resultBefore = await BeforeCreateUpdate(moduleActivity, task, modelState, appUser.TenantLanguage, _moduleRepository, _picklistRepository, _profileRepository, appUser: appUser);

							if (resultBefore < 0 && !modelState.IsValid)
							{
								ErrorHandler.LogError(new Exception("Task cannot be created! Object: " + task + " ModelState: " + modelState.ToJsonString()), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
								return;
							}

							try
							{
								var resultCreate = await _recordRepository.Create(task, moduleActivity);

								if (resultCreate < 1)
								{
									ErrorHandler.LogError(new Exception("Task cannot be created! Object: " + task), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
									return;
								}

								AfterCreate(moduleActivity, task, appUser, warehouse, moduleActivity.Id != module.Id);
							}
							catch (Exception ex)
							{
								ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
								return;
							}
						}

						if (workflow.WebHook != null)
						{
							try
							{
								var webHook = workflow.WebHook;

								using (var client = new HttpClient())
								{
									/*
                                     * If the server only supports higher TLS version like TLS 1.2 only, it will still fail unless your client PC is configured to use higher TLS version by default. 
                                     * To overcome this problem add the following in your code.
                                     */
									System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;

									var recordId = (int)record["id"];
									var jsonData = new JObject();
									jsonData["id"] = recordId;

									if (webHook.Parameters != null)
									{
										var data = webHook.Parameters.Split(',');
										lookupModuleNames = new List<string>();

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

										var webhookFormatNumeric = true;
										var webhookRecordFormatSetting = await _settingRepository.GetByKeyAsync("webhook_format_numeric");

										if (webhookRecordFormatSetting != null)
											webhookFormatNumeric = bool.Parse(webhookRecordFormatSetting.Value);

										var recordData = _recordRepository.GetById(module, recordId, false, lookupModules, true);
										recordData = await Model.Helpers.RecordHelper.FormatRecordValues(module, recordData, _moduleRepository, _picklistRepository, _configuration, appUser.TenantGuid, appUser.Language, appUser.Culture, 180, lookupModules, false, webhookFormatNumeric);

										foreach (var dataString in data)
										{
											var dataObject = dataString.Split('|');
											var parameterName = dataObject[0];
											var moduleName = dataObject[1];
											var fieldName = dataObject[2];

											if (moduleName != module.Name)
												jsonData[parameterName] = recordData[moduleName + "." + fieldName];
											else
											{
												var currentField = module.Fields.Single(x => x.Name == fieldName);

												if (currentField.DataType == DataType.Lookup && currentField.LookupType == "users")
													jsonData[parameterName] = recordData[fieldName + ".full_name"];
												else
													jsonData[parameterName] = recordData[fieldName];
											}
										}
									}

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

												//await client.PostAsJsonAsync(webHook.CallbackUrl, jsonData);
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
								ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
							}
						}

						if (workflow.SendNotification != null)
						{
							if (workflow.FieldUpdate != null)
							{
								lookupModuleNames = new List<string>();
								lookupModules = null;

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

							var sendNotification = workflow.SendNotification;
							var recipients = sendNotification.RecipientsArray;
							var sendNotificationCC = sendNotification.CC;
							var sendNotificationBCC = sendNotification.Bcc;

							if (sendNotification.CCArray != null && sendNotification.CCArray.Length == 1 && !sendNotification.CC.Contains("@"))
							{
								sendNotificationCC = (string)record[sendNotification.CC];
							}

							if (sendNotification.BccArray != null && sendNotification.BccArray.Length == 1 && !sendNotification.Bcc.Contains("@"))
							{
								sendNotificationBCC = (string)record[sendNotification.Bcc];
							}

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

							var subdomain = _configuration.GetSection("TestMode")["BlobUrl"] == "true" ? "test" : appDomain;

							domain = string.Format(domain, subdomain);

							//domain = "http://localhost:5554/";

							using (var _appRepository = new ApplicationRepository(platformDatabaseContext, _configuration))
							{
								var app = await _appRepository.Get(appUser.AppId);
								if (app != null)
								{
									domain = "http://" + app.Setting.AppDomain + "/";
								}
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
									using (var userRepository = new UserRepository(databaseContext, _configuration))
									{
										userRepository.CurrentUser = _currentUser;
										var recipientUser = await userRepository.GetById((int)record["owner.id"]);

										if (recipientUser == null)
											continue;

										recipient = recipientUser.Email;
									}

									if (recipients.Contains(recipient))
										continue;
								}

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

						}

						var workflowLog = new WorkflowLog
						{
							WorkflowId = workflow.Id,
							ModuleId = module.Id,
							RecordId = (int)record["id"]
						};

						try
						{
							var resultCreateLog = await _workflowRepository.CreateLog(workflowLog);

							if (resultCreateLog < 1)
								ErrorHandler.LogError(new Exception("WorkflowLog cannot be created! Object: " + workflowLog.ToJsonString()), "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
						}
						catch (Exception ex)
						{
							ErrorHandler.LogError(ex, "email: " + appUser.Email + " " + "tenant_id:" + appUser.TenantId + "module_name:" + module.Name + "operation_type:" + operationType + "record_id:" + record["id"].ToString());
						}

					}
				}
			}
		}

		public async Task<Workflow> CreateEntity(WorkflowBindingModel workflowModel, string tenantLanguage)
		{
			var workflow = new Workflow
			{
				ModuleId = workflowModel.ModuleId,
				Name = workflowModel.Name,
				Frequency = workflowModel.Frequency,
				Active = workflowModel.Active,
				OperationsArray = workflowModel.Operations,
				ProcessFilter = workflowModel.ProcessFilter,
				ChangedField = workflowModel.ChangedField
			};
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
				using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
				{
					_moduleRepository.CurrentUser = _picklistRepository.CurrentUser = _currentUser;

					if (workflowModel.Filters != null && workflowModel.Filters.Count > 0)
					{
						var module = await _moduleRepository.GetById(workflowModel.ModuleId);
						var picklistItemIds = new List<int>();
						workflow.Filters = new List<WorkflowFilter>();

						foreach (var filterModel in workflowModel.Filters)
						{
							var field = module.Fields.Single(x => x.Name == filterModel.Field);

							if (field.DataType == DataType.Picklist)
							{
								int tempvalue;
								if (int.TryParse(filterModel.Value.ToString(), out tempvalue))
								{
									picklistItemIds.Add(int.Parse(filterModel.Value.ToString()));
								}

								else
								{
									picklistItemIds.Add(char.Parse(filterModel.Value.ToString()));
								}
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

						foreach (var filterModel in workflowModel.Filters)
						{
							var field = module.Fields.Single(x => x.Name == filterModel.Field);
							var value = filterModel.Value.ToString();

							if (filterModel.Operator != Operator.Empty && filterModel.Operator != Operator.NotEmpty)
							{
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

								else if (field.DataType == DataType.Tag)
								{

								}
							}

							var filter = new WorkflowFilter
							{
								Field = filterModel.Field,
								Operator = filterModel.Operator,
								Value = value,
								No = filterModel.No
							};

							workflow.Filters.Add(filter);
						}
					}

					if (workflowModel.Actions.SendNotification != null)
					{
						workflow.SendNotification = new WorkflowNotification
						{
							Subject = workflowModel.Actions.SendNotification.Subject,
							Message = workflowModel.Actions.SendNotification.Message,
							RecipientsArray = workflowModel.Actions.SendNotification.Recipients,
							CCArray = workflowModel.Actions.SendNotification.CC,
							BccArray = workflowModel.Actions.SendNotification.Bcc,
							Schedule = workflowModel.Actions.SendNotification.Schedule
						};
					}

					if (workflowModel.Actions.CreateTask != null)
					{
						workflow.CreateTask = new WorkflowTask
						{
							Owner = workflowModel.Actions.CreateTask.Owner,
							Subject = workflowModel.Actions.CreateTask.Subject,
							TaskDueDate = workflowModel.Actions.CreateTask.TaskDueDate,
							TaskStatus = workflowModel.Actions.CreateTask.TaskStatus,
							TaskPriority = workflowModel.Actions.CreateTask.TaskPriority,
							TaskNotification = workflowModel.Actions.CreateTask.TaskNotification,
							TaskReminder = workflowModel.Actions.CreateTask.TaskReminder,
							ReminderRecurrence = workflowModel.Actions.CreateTask.ReminderRecurrence,
							Description = workflowModel.Actions.CreateTask.Description
						};
					}

					if (workflowModel.Actions.FieldUpdate != null)
					{
						workflow.FieldUpdate = new WorkflowUpdate
						{
							Module = workflowModel.Actions.FieldUpdate.Module,
							Field = workflowModel.Actions.FieldUpdate.Field,
							Value = workflowModel.Actions.FieldUpdate.Value
						};
					}

					if (workflowModel.Actions.WebHook != null)
					{
						workflow.WebHook = new WorkflowWebhook
						{
							CallbackUrl = workflowModel.Actions.WebHook.CallbackUrl,
							MethodType = workflowModel.Actions.WebHook.MethodType,
							Parameters = workflowModel.Actions.WebHook.Parameters
						};
					}

					return workflow;
				}
			}
		}

		public async Task UpdateEntity(WorkflowBindingModel workflowModel, Workflow workflow, string tenantLanguage)
		{
			workflow.Name = workflowModel.Name;
			workflow.Frequency = workflowModel.Frequency;
			workflow.ProcessFilter = workflowModel.ProcessFilter;
			workflow.Active = workflowModel.Active;
			workflow.OperationsArray = workflowModel.Operations;
			workflow.ChangedField = workflowModel.ChangedField;

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
				using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
				{
					_moduleRepository.CurrentUser = _picklistRepository.CurrentUser = _currentUser;

					if (workflowModel.Filters != null && workflowModel.Filters.Count > 0)
					{
						if (workflow.Filters == null)
							workflow.Filters = new List<WorkflowFilter>();

						var module = await _moduleRepository.GetById(workflowModel.ModuleId);
						var picklistItemIds = new List<int>();

						foreach (var filterModel in workflowModel.Filters)
						{
							if (filterModel.Operator != Operator.Empty && filterModel.Operator != Operator.NotEmpty)
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
						}

						ICollection<PicklistItem> picklistItems = null;

						if (picklistItemIds.Count > 0)
							picklistItems = await _picklistRepository.FindItems(picklistItemIds);

						foreach (var filterModel in workflowModel.Filters)
						{
							var field = module.Fields.Single(x => x.Name == filterModel.Field);
							var value = filterModel.Value.ToString();

							if (filterModel.Operator != Operator.Empty && filterModel.Operator != Operator.NotEmpty)
							{
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
							}

							var filter = new WorkflowFilter
							{
								Field = filterModel.Field,
								Operator = filterModel.Operator,
								Value = value,
								No = filterModel.No
							};

							workflow.Filters.Add(filter);
						}
					}

					if (workflowModel.Actions.SendNotification != null)
					{
						if (workflow.SendNotification == null)
							workflow.SendNotification = new WorkflowNotification();

						workflow.SendNotification.Subject = workflowModel.Actions.SendNotification.Subject;
						workflow.SendNotification.Message = workflowModel.Actions.SendNotification.Message;
						workflow.SendNotification.RecipientsArray = workflowModel.Actions.SendNotification.Recipients;
						if (workflowModel.Actions.SendNotification.CC.Length > 0)
							workflow.SendNotification.CCArray = workflowModel.Actions.SendNotification.CC;
						else
						{
							string[] emptyCC = new string[] { "" };
							workflow.SendNotification.CCArray = emptyCC;
						}
						if (workflowModel.Actions.SendNotification.Bcc.Length > 0)
							workflow.SendNotification.BccArray = workflowModel.Actions.SendNotification.Bcc;
						else
						{
							string[] emptyBcc = new string[] { "" };
							workflow.SendNotification.BccArray = emptyBcc;
						}
						//Bcc ve Cc silinip güncellendiğinde boş setlemiyordu.
						//workflow.SendNotification.BccArray = workflowModel.Actions.SendNotification.Bcc;
						//workflow.SendNotification.CCArray = workflowModel.Actions.SendNotification.CC;
						workflow.SendNotification.Schedule = workflowModel.Actions.SendNotification.Schedule;
					}
					else
					{
						workflow.SendNotification = null;
					}

					if (workflowModel.Actions.CreateTask != null)
					{
						if (workflow.CreateTask == null)
							workflow.CreateTask = new WorkflowTask();

						workflow.CreateTask.Owner = workflowModel.Actions.CreateTask.Owner;
						workflow.CreateTask.Subject = workflowModel.Actions.CreateTask.Subject;
						workflow.CreateTask.TaskDueDate = workflowModel.Actions.CreateTask.TaskDueDate;
						workflow.CreateTask.TaskStatus = workflowModel.Actions.CreateTask.TaskStatus;
						workflow.CreateTask.TaskPriority = workflowModel.Actions.CreateTask.TaskPriority;
						workflow.CreateTask.TaskNotification = workflowModel.Actions.CreateTask.TaskNotification;
						workflow.CreateTask.TaskReminder = workflowModel.Actions.CreateTask.TaskReminder;
						workflow.CreateTask.ReminderRecurrence = workflowModel.Actions.CreateTask.ReminderRecurrence;
						workflow.CreateTask.Description = workflowModel.Actions.CreateTask.Description;
					}
					else
					{
						workflow.CreateTask = null;
					}

					if (workflowModel.Actions.FieldUpdate != null)
					{
						if (workflow.FieldUpdate == null)
							workflow.FieldUpdate = new WorkflowUpdate();

						workflow.FieldUpdate.Module = workflowModel.Actions.FieldUpdate.Module;
						workflow.FieldUpdate.Field = workflowModel.Actions.FieldUpdate.Field;
						workflow.FieldUpdate.Value = workflowModel.Actions.FieldUpdate.Value;
					}
					else
					{
						workflow.FieldUpdate = null;
					}

					if (workflowModel.Actions.WebHook != null)
					{
						if (workflow.WebHook == null)
							workflow.WebHook = new WorkflowWebhook();

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
	}
}