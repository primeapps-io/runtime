using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.Notifications;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Record;
using Microsoft.AspNetCore.Http;
using ModelModuleHelper = PrimeApps.Model.Helpers.ModuleHelper;
using PrimeApps.App.Services;
using PrimeApps.Model.Entities.Platform;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using Microsoft.Extensions.Configuration;
using WorkflowCore.Interface;
using System.Text.RegularExpressions;

namespace PrimeApps.App.Helpers
{
	public interface IRecordHelper
	{
		Task<int> BeforeCreateUpdate(Module module, JObject record, ModelStateDictionary modelState, string tenantLanguage, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IProfileRepository profileRepository, bool convertPicklists = true, JObject currentRecord = null, UserItem appUser = null);
		Task<int> BeforeDelete(Module module, JObject record, UserItem appUser, IProcessRepository processRepository, IProfileRepository profileRepository, ModelStateDictionary modelState, Warehouse warehouse);
		void AfterCreate(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180, bool runDefaults = true);
		void AfterUpdate(Module module, JObject record, JObject currentRecord, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180);
		void AfterDelete(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180);
		JObject PrepareConflictError(PostgresException ex);
		bool ValidateFilterLogic(string filterLogic, List<Filter> filters);
		Task CreateStageHistory(JObject record, JObject currentRecord = null);
		Task UpdateStageHistory(JObject record, JObject currentRecord);
		Task SetCombinations(JObject record, IModuleRepository _moduleRepository, string culture, JObject currentRecord, Field fieldCombination, int timeZoneOffset = 180);
		Task SetActivityType(JObject record);
		Task SetTransactionType(JObject record);
		Task SetForecastFields(JObject record);
		JObject CreateStageHistoryRecord(JObject record, JObject currentRecord);
		string GetRecordPrimaryValue(JObject record, Module module);
		Task<List<string>> GetAllFieldsForFindRequest(string moduleName, bool withLookups = true);
		void SetCurrentUser(UserItem appUser);
	}

	public delegate Task<int> BeforeCreateUpdate(Module module, JObject record, ModelStateDictionary modelState, string tenantLanguage, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IProfileRepository profileRepository, bool convertPicklists = true, JObject currentRecord = null, UserItem appUser = null);

	public delegate Task UpdateStageHistory(JObject record, JObject currentRecord);

	public delegate void AfterUpdate(Module module, JObject record, JObject currentRecord, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180);

	public delegate void AfterCreate(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180, bool runDefaults = true);

	public delegate Task<List<string>> GetAllFieldsForFindRequest(string moduleName, bool withLookups = true);

	public delegate bool ValidateFilterLogic(string filterLogic, List<Filter> filters);

	public class RecordHelper : IRecordHelper, IDisposable
	{
		private CurrentUser _currentUser;
		private IServiceScopeFactory _serviceScopeFactory;
		private IConfiguration _configuration;
		private IAuditLogHelper _auditLogHelper;
		private INotificationHelper _notificationHelper;
		private IWorkflowHelper _workflowHelper;
		private IProcessHelper _processHelper;
		private ICalculationHelper _calculationHelper;
		private IChangeLogHelper _changeLogHelper;
		private IBpmHelper _bpmHelper;
		private IHttpContextAccessor _context;
		private IWorkflowHost _workflowHost;
		private IModuleRepository _moduleRepository;

		public IBackgroundTaskQueue Queue { get; }

		public RecordHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IAuditLogHelper auditLogHelper, INotificationHelper notificationHelper, IWorkflowHelper workflowHelper, IProcessHelper processHelper, ICalculationHelper calculationHelper, IChangeLogHelper changeLogHelper, IBpmHelper bpmHelper, IBackgroundTaskQueue queue, IHttpContextAccessor context, IWorkflowHost workflowHost, IModuleRepository moduleRepository)
		{
			_context = context;
			_serviceScopeFactory = serviceScopeFactory;
			_configuration = configuration;
			_currentUser = UserHelper.GetCurrentUser(_context);
			_auditLogHelper = auditLogHelper;
			_notificationHelper = notificationHelper;
			_workflowHelper = workflowHelper;
			_processHelper = processHelper;
			_calculationHelper = calculationHelper;
			_changeLogHelper = changeLogHelper;
			_bpmHelper = bpmHelper;
			_workflowHost = workflowHost;
			_moduleRepository = moduleRepository;

			Queue = queue;
		}

		public RecordHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, CurrentUser currentUser)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_configuration = configuration;

			_currentUser = currentUser;
			_auditLogHelper = new AuditLogHelper(configuration, serviceScopeFactory, currentUser);
			_notificationHelper = new NotificationHelper(configuration, serviceScopeFactory, currentUser);
			_workflowHelper = new WorkflowHelper(configuration, serviceScopeFactory, currentUser);
			_processHelper = new ProcessHelper(configuration, serviceScopeFactory, currentUser);
			_calculationHelper = new CalculationHelper(configuration, serviceScopeFactory, currentUser);
			_bpmHelper = new BpmHelper(configuration, serviceScopeFactory, currentUser);
			//TODO LogHelper
			_changeLogHelper = new ChangeLogHelper();

			Queue = new BackgroundTaskQueue();
		}

		public async Task<int> BeforeCreateUpdate(Module module, JObject record, ModelStateDictionary modelState, string tenantLanguage, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IProfileRepository profileRepository, bool convertPicklists = true, JObject currentRecord = null, UserItem appUser = null)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
				using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
				{
					_moduleRepository.CurrentUser = _picklistRepository.CurrentUser = _currentUser;
					//TODO: Validate metadata
					//TODO: Profile permission check
					var operationUpdate = !record["id"].IsNullOrEmpty();
					var picklistItemIds = new List<int>();
					JObject recordNew = new JObject();

					// Check all fields is valid and prepare for related data types
					foreach (var prop in record)
					{
						if (ModelModuleHelper.SystemFields.Contains(prop.Key))
							continue;

						var specificFieldList = ModelModuleHelper.ModuleSpecificFields(module);

						if (specificFieldList.Count() > 0 && specificFieldList.Contains(prop.Key))
							continue;

						var field = module.Fields.FirstOrDefault(x => x.Name == prop.Key);

						if (field == null)
						{
							modelState.AddModelError(prop.Key, $"Field '{prop.Key}' not found.");
							return StatusCodes.Status400BadRequest;
						}

						if (prop.Value.IsNullOrEmpty())
							continue;


						if (field.Encrypted)
						{
							recordNew[prop.Key] = null;
							recordNew[prop.Key + "__encrypted"] = EncryptionHelper.Encrypt((string)prop.Value, field.Id, appUser, _configuration);
						}

						if (field.DataType == DataType.Picklist && convertPicklists)
						{
							if (!prop.Value.IsNumeric())
							{
								modelState.AddModelError(prop.Key, $"Value of '{prop.Key}' field must be numeric.");
								return StatusCodes.Status400BadRequest;
							}

							picklistItemIds.Add((int)prop.Value);
						}
						if (field.DataType == DataType.Tag)
						{
							var tagLabel = new List<string>();

							string str = (string)prop.Value;

							string[] tags = str.Split(',');


							using (var _tagRepository = new TagRepository(databaseContext, _configuration))
							{
								_tagRepository.CurrentUser = _currentUser;
								var fieldTags = await _tagRepository.GetByFieldId(field.Id);

								foreach (string tag in tags)
								{
									tagLabel.Add(tag);
									var fieldTag = fieldTags.Where(x => x.Text == tag).ToList();

									if (fieldTag.Count <= 0)
									{
										var creatTag = new Tag
										{
											Text = tag,
											FieldId = field.Id
										};

										await _tagRepository.Create(creatTag);
									}
								}
							}


							record[prop.Key] = string.Join("|", tagLabel);
						}
						if (field.DataType == DataType.Multiselect && convertPicklists)
						{
							if (!(prop.Value is JArray))
							{
								modelState.AddModelError(prop.Key, $"Value of '{prop.Key}' field must be array.");
								return StatusCodes.Status400BadRequest;
							}

							var multiselectPicklistItemIds = new List<int>();

							foreach (var item in (JArray)prop.Value)
							{
								if (!item.IsNumeric())
								{
									modelState.AddModelError(prop.Key, $"All items of '{prop.Key}' field value must be numeric.");
									return StatusCodes.Status400BadRequest;
								}

								multiselectPicklistItemIds.Add((int)item);
							}

							picklistItemIds.AddRange(multiselectPicklistItemIds);
						}
					}

					if (!recordNew.IsNullOrEmpty())
					{
						foreach (var prop in recordNew)
						{
							record[prop.Key] = recordNew[prop.Key];
						}
					}

					//Module specific actions
					switch (module.Name)
					{
						case "activities":
							if (!record["activity_type"].IsNullOrEmpty())
								await SetActivityType(record);
							break;
						case "opportunities":
							await SetForecastFields(record);
							break;
						case "current_accounts":
							await SetTransactionType(record);
							break;
					}

					// Check picklists and set picklist's label to record value
					if (picklistItemIds.Count > 0 && convertPicklists)
					{
						var picklistItems = await _picklistRepository.FindItems(picklistItemIds);
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
									var relatedModule = await _moduleRepository.GetByIdBasic((int)pair.Value - 900000);

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

					if (module.Name != "sales_orders")
					{
						//Validate metadata
						var moduleFields = module.Fields.Where(x => !x.Deleted && x.DataType != DataType.NumberAuto).ToList();

						IDictionary<string, JToken> dictionary = record;

						for (int i = 0; i < moduleFields.Count; i++)
						{
							var moduleField = moduleFields[i];

							if (ModelModuleHelper.SystemFieldsExtended.Contains(moduleField.Name))
								continue;

							if (FieldHasDependencyOrCombination(module, moduleField, record, tenantLanguage, picklistRepository))
								continue;

							if (moduleField.Validation != null)
							{
								if (moduleField.Validation.Required != null && (bool)moduleField.Validation.Required &&
								((!operationUpdate && record[moduleField.Name].IsNullOrEmpty()) ||
								(operationUpdate && dictionary.ContainsKey(moduleField.Name) && record[moduleField.Name].IsNullOrEmpty())))
								{
									modelState.AddModelError(moduleField.Name, $"Field '{moduleField.Name}' is required.");
									return StatusCodes.Status400BadRequest;
								}

								if (moduleField.Validation.Min != null && !record[moduleField.Name].IsNullOrEmpty() && int.Parse((string)record[moduleField.Name]) < moduleField.Validation.Min)
								{
									modelState.AddModelError(moduleField.Name, $"Field '{moduleField.Name}' minimum value must be {moduleField.Validation.Min}.");
									return StatusCodes.Status400BadRequest;
								}

								if (moduleField.Validation.Max != null && !record[moduleField.Name].IsNullOrEmpty() && int.Parse((string)record[moduleField.Name]) > moduleField.Validation.Max)
								{
									modelState.AddModelError(moduleField.Name, $"Field '{moduleField.Name}' maximum value must be {moduleField.Validation.Max}.");
									return StatusCodes.Status400BadRequest;
								}

								if (moduleField.Validation.MinLength != null && !record[moduleField.Name].IsNullOrEmpty() && record[moduleField.Name].ToString().Length < moduleField.Validation.MinLength)
								{
									modelState.AddModelError(moduleField.Name, $"Field '{moduleField.Name}' minimum length must be {moduleField.Validation.MinLength}.");
									return StatusCodes.Status400BadRequest;
								}

								if (moduleField.Validation.MaxLength != null && !record[moduleField.Name].IsNullOrEmpty() && record[moduleField.Name].ToString().Length > moduleField.Validation.MaxLength)
								{
									modelState.AddModelError(moduleField.Name, $"Field '{moduleField.Name}' minimum length must be {moduleField.Validation.MaxLength}.");
									return StatusCodes.Status400BadRequest;
								}

								if (moduleField.Validation.Pattern != null && !record[moduleField.Name].IsNullOrEmpty())
								{
									Match match = Regex.Match((string)record[moduleField.Name], "^" + moduleField.Validation.Pattern + "$", RegexOptions.IgnoreCase);
									if (!match.Success)
									{
										modelState.AddModelError(moduleField.Name, $"Field '{moduleField.Name}' regex not match. Regex template: {moduleField.Validation.Pattern}");
										return StatusCodes.Status400BadRequest;
									}
								}
							}
						}
					}

					//Check profile permissions

					var userProfile = await profileRepository.GetProfileById(appUser.ProfileId);
					var modulePermission = userProfile.Permissions.Where(x => x.ModuleId == module.Id).FirstOrDefault();

					if (modulePermission == null)
					{
						modelState.AddModelError(module.Name, $"You dont have profile permission for '{module.Name}' module.");
						return StatusCodes.Status403Forbidden;
					}

					if (!record["id"].IsNullOrEmpty() && !modulePermission.Modify)
					{
						modelState.AddModelError(module.Name, $"You dont have profile permission for update '{module.Name}' module.");
						return StatusCodes.Status403Forbidden;
					}

					if (record["id"].IsNullOrEmpty() && !modulePermission.Write)
					{
						modelState.AddModelError(module.Name, $"You dont have profile permission for create '{module.Name}' module.");
						return StatusCodes.Status403Forbidden;
					}

					/*
					 * Birleşim data tipi başka bir birleşim data tipiyle kullanılması durumu.
					 * Bu durum oluştuğunda birleşim de kullanılan diğer birleşim alanı henüz hesaplanmamış ise
					 * Önceki birleşim alanı eksik doluyordu.
					 * Sorunu çözmek için birleşim alanlarının sayısı kadar SetCombinations metodunu çalıştırıyoruz.
					 * Bu sayede elde edilen sonuç her zaman doğru oluyor.
					 */

					// Process combination
					var fieldCombinations = module.Fields.Where(x => x.Combination != null);

					for (int i = 0; i < fieldCombinations.Count(); i++)
					{
						foreach (var fieldCombination in fieldCombinations)
						{
							await SetCombinations(record, moduleRepository, appUser.Culture, currentRecord, fieldCombination, 180);
						}
					}

					// Check freeze
					if (!currentRecord.IsNullOrEmpty() && !currentRecord["process_id"].IsNullOrEmpty())
					{
						if ((int)currentRecord["process_status"] != 3 && (int)currentRecord["operation_type"] != 1 && appUser != null && !appUser.HasAdminProfile)
							record["freeze"] = true;
						else
							record["freeze"] = false;
					}
					return StatusCodes.Status200OK;
				}
			}
		}

		public async Task<int> BeforeDelete(Module module, JObject record, UserItem appUser, IProcessRepository processRepository, IProfileRepository profileRepository, ModelStateDictionary modelState, Warehouse warehouse)
		{
			//Check profile permissions

			var userProfile = await profileRepository.GetProfileById(appUser.ProfileId);
			var modulePermission = userProfile.Permissions.Where(x => x.ModuleId == module.Id).FirstOrDefault();

			if (modulePermission == null)
			{
				modelState.AddModelError(module.Name, $"You dont have profile permission for '{module.Name}' module.");
				return StatusCodes.Status403Forbidden;
			}

			if (!modulePermission.Remove)
			{
				modelState.AddModelError(module.Name, $"You dont have profile permission for delete '{module.Name}' module.");
				return StatusCodes.Status403Forbidden;
			}
			// Check freeze
			if (!record.IsNullOrEmpty() && !record["process_id"].IsNullOrEmpty())
			{
				var process = await processRepository.GetById((int)record["process_id"]);
				record["freeze"] = true;

				if (appUser != null)
				{
					if (appUser.HasAdminProfile)
					{
						record["freeze"] = false;
					}
					else
					{
						if (process.Profiles != "" && process.Profiles != null)
						{
							var profiles = process.Profiles.Split(',').Select(Int32.Parse).ToList();

							foreach (var item in profiles)
							{
								if (item == appUser.ProfileId)
								{
									record["freeze"] = false;
									break;
								}
							}
						}
						else
						{
							record["freeze"] = true;
						}
					}
				}
				if (module.Name == "izinler")
					await _calculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.insert, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest);
			}

			return StatusCodes.Status200OK;
		}

		public void AfterCreate(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180, bool runDefaults = true)
		{
			if (runDefaults)
			{
				Queue.QueueBackgroundWorkItem(async token => await _auditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(record, module), AuditType.Record, RecordActionType.Inserted, null, module));
				Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.Create(appUser, record, module, warehouse, timeZoneOffset));
				Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.SendTaskNotification(record, appUser, module));
			}

			if (runWorkflows)
			{
				Queue.QueueBackgroundWorkItem(async token => await _workflowHelper.Run(OperationType.insert, record, module, appUser, warehouse, BeforeCreateUpdate, UpdateStageHistory, AfterUpdate, AfterCreate));
				Queue.QueueBackgroundWorkItem(async token => await _processHelper.Run(OperationType.insert, record, module, appUser, warehouse, ProcessTriggerTime.Instant, BeforeCreateUpdate, GetAllFieldsForFindRequest, UpdateStageHistory, AfterUpdate, AfterCreate));
				Queue.QueueBackgroundWorkItem(async token => await _bpmHelper.Run(OperationType.insert, record, module, appUser, warehouse));
			}


			if (runCalculations)
			{
				Queue.QueueBackgroundWorkItem(async token => await _calculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.insert, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest));
			}
		}

		public void AfterUpdate(Module module, JObject record, JObject currentRecord, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180)
		{
			Queue.QueueBackgroundWorkItem(async token => await _changeLogHelper.CreateLog(appUser, currentRecord, module));
			Queue.QueueBackgroundWorkItem(async token => await _auditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(currentRecord, module), AuditType.Record, RecordActionType.Updated, null, module));
			Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.Update(appUser, record, currentRecord, module, warehouse, timeZoneOffset));
			Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.SendTaskNotification(record, appUser, module));

			if (runWorkflows)
			{
				Queue.QueueBackgroundWorkItem(async token => await _workflowHelper.Run(OperationType.update, record, module, appUser, warehouse, BeforeCreateUpdate, UpdateStageHistory, AfterUpdate, AfterCreate, currentRecord));
				Queue.QueueBackgroundWorkItem(async token => await _processHelper.Run(OperationType.update, record, module, appUser, warehouse, ProcessTriggerTime.Instant, BeforeCreateUpdate, GetAllFieldsForFindRequest, UpdateStageHistory, AfterUpdate, AfterCreate));
				Queue.QueueBackgroundWorkItem(async token => await _bpmHelper.Run(OperationType.update, record, module, appUser, warehouse));

				//_workflowHost.PublishEvent("record_update", record["id"].ToString(), record["id"].ToString()).GetAwaiter();
			}


			if (runCalculations)
			{
				Queue.QueueBackgroundWorkItem(async token => await _calculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.update, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest, currentRecord));
			}
		}

		public void AfterDelete(Module module, JObject record, UserItem appUser, Warehouse warehouse, bool runWorkflows = true, bool runCalculations = true, int timeZoneOffset = 180)
		{
			Queue.QueueBackgroundWorkItem(async token => await _auditLogHelper.CreateLog(appUser, (int)record["id"], GetRecordPrimaryValue(record, module), AuditType.Record, RecordActionType.Deleted, null, module));
			Queue.QueueBackgroundWorkItem(async token => await _notificationHelper.Delete(appUser, record, module, timeZoneOffset));

			if (runWorkflows)
			{
				Queue.QueueBackgroundWorkItem(async token => await _workflowHelper.Run(OperationType.delete, record, module, appUser, warehouse, BeforeCreateUpdate, UpdateStageHistory, AfterUpdate, AfterCreate));
				Queue.QueueBackgroundWorkItem(async token => await _processHelper.Run(OperationType.delete, record, module, appUser, warehouse, ProcessTriggerTime.Instant, BeforeCreateUpdate, GetAllFieldsForFindRequest, UpdateStageHistory, AfterUpdate, AfterCreate));
				Queue.QueueBackgroundWorkItem(async token => await _bpmHelper.Run(OperationType.delete, record, module, appUser, warehouse));
			}


			if (runCalculations)
			{
				Queue.QueueBackgroundWorkItem(async token => await _calculationHelper.Calculate((int)record["id"], module, appUser, warehouse, OperationType.delete, BeforeCreateUpdate, AfterUpdate, GetAllFieldsForFindRequest));
			}
		}

		public JObject PrepareConflictError(PostgresException ex)
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

		public bool ValidateFilterLogic(string filterLogic, List<Filter> filters)
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

		public async Task CreateStageHistory(JObject record, JObject currentRecord = null)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
				using (var _recordRepository = new RecordRepository(databaseContext, _configuration))
				{
					_moduleRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;
					if (record["stage"] != null)
					{
						var stageHistoryModule = await _moduleRepository.GetByNameBasic("stage_history");
						var stageHistory = CreateStageHistoryRecord(record, currentRecord);

						await _recordRepository.Create(stageHistory, stageHistoryModule);
					}
				}
			}

		}

		public async Task UpdateStageHistory(JObject record, JObject currentRecord)
		{
			if (record["stage"] != null)
			{
				if ((string)record["stage"] != (string)currentRecord["stage"])
					await CreateStageHistory(record, currentRecord);
			}
		}

		public async Task SetCombinations(JObject record, IModuleRepository _moduleRepository, string currentCulture, JObject currentRecord, Field fieldCombination, int timeZoneOffset = 180)
		{
			var field1 = await _moduleRepository.GetFieldByName(fieldCombination.Combination.Field1);
			var field2 = await _moduleRepository.GetFieldByName(fieldCombination.Combination.Field2);
			var formatDate = currentCulture == "tr-TR" ? "dd.MM.yyyy" : "M/d/yyyy";
			var formatDateTime = currentCulture == "tr-TR" ? "dd.MM.yyyy HH:mm" : "M/d/yyyy h:mm a";
			if ((field1 != null && (field1.DataType == DataType.Date || field1.DataType == DataType.DateTime)) || (field2 != null && (field2.DataType == DataType.Date || field2.DataType == DataType.DateTime)))
			{
				if (!record[fieldCombination.Combination.Field1].IsNullOrEmpty())
				{
					switch (field1.DataType)
					{
						case DataType.Date:
							record[fieldCombination.Combination.Field1] = ((string)record[fieldCombination.Combination.Field1]).Length < 11 ? record[fieldCombination.Combination.Field1] : ((DateTime)record[fieldCombination.Combination.Field1]).AddMinutes(timeZoneOffset).ToString(formatDate);
							break;
						case DataType.DateTime:
							record[fieldCombination.Combination.Field1] = ((string)record[fieldCombination.Combination.Field1]).Length < 17 ? record[fieldCombination.Combination.Field1] : ((DateTime)record[fieldCombination.Combination.Field1]).AddMinutes(timeZoneOffset).ToString(formatDateTime);
							break;
					}
				}
				else if (!record[fieldCombination.Combination.Field2].IsNullOrEmpty())
				{
					switch (field2.DataType)
					{
						case DataType.Date:
							record[fieldCombination.Combination.Field2] = ((string)record[fieldCombination.Combination.Field2]).Length < 11 ? record[fieldCombination.Combination.Field2] : ((DateTime)record[fieldCombination.Combination.Field2]).ToString(formatDate);
							break;
						case DataType.DateTime:
							record[fieldCombination.Combination.Field2] = ((string)record[fieldCombination.Combination.Field2]).Length < 17 ? record[fieldCombination.Combination.Field2] : ((DateTime)record[fieldCombination.Combination.Field2]).AddMinutes(timeZoneOffset).ToString(formatDateTime);
							break;
					}
				}
			}
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

		public async Task SetActivityType(JObject record)
		{
			if (!record["id"].IsNullOrEmpty() && record["activity_type"].IsNullOrEmpty())
				return;

			if (record["id"].IsNullOrEmpty() && record["activity_type"].IsNullOrEmpty())
				throw new Exception("ActivityType not found!");

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
				{
					_picklistRepository.CurrentUser = _currentUser;
					var activityType = await _picklistRepository.GetItemById((int)record["activity_type"]);

					if (activityType == null)
						throw new Exception("ActivityType picklist item not found!");

					record["activity_type_system"] = activityType.Value;
				}
			}
		}

		public static bool FieldHasDependencyOrCombination(Module module, Field field, JObject record, string tenantLanguage, IPicklistRepository picklistRepository)
		{
			var dependencies = module.Dependencies.Where(x => !x.Deleted && x.DependencyType == DependencyType.Display && x.ChildField == field.Name).ToList();

			if (dependencies.Count > 0)
				return true;

			if (field.Combination != null)
				return true;

			return false;

			/*
             * Dependencies kontrolleri düzgün yazılıp açılabilir. 
             * Şimdilik field üzerinden dependency veya combination varsa backend de validasyonları kontrol etmiyoruz.
             */
			/*foreach (var dependency in dependencies)
            {
                var parentDependency = dependencies.Where(x => x.ChildField == dependency.ParentField).ToList();

                if (parentDependency.Count > 0)
                {

                }
                else
                {
                    var parentField = module.Fields.Where(x => !x.Deleted && x.Name == dependency.ParentField).FirstOrDefault();
                    if (parentField.DataType == DataType.Picklist)
                    {
                        var picklistValues = new System.Collections.Generic.List<string>();
                        var picklist = await picklistRepository.GetById((int)parentField.PicklistId);

                        foreach (var value in dependency.ValuesArray)
                        {
                            var selectedOption = picklist.Items.Where(x => x.Id == int.Parse(value)).FirstOrDefault();
                            picklistValues.Add(tenantLanguage == "en" ? selectedOption.LabelEn : selectedOption.LabelTr);
                        }

                        if (record[parentField.Name] != null && picklistValues.Contains(record[parentField.Name].ToString()))
                            return !dependency.Otherwise;

                        return dependency.Otherwise;
                    }
                    else if (parentField.DataType == DataType.Checkbox)
                    {
                        if (!record[parentField.Name].IsNullOrEmpty())
                        {
                            return dependency.Otherwise ? !(bool)record[parentField.Name] : (bool)record[parentField.Name];
                        }

                        return dependency.Otherwise;

                    }
                }
            }
            return true;
            */
		}

		public async Task SetTransactionType(JObject record)
		{
			if (!record["id"].IsNullOrEmpty() && record["transaction_type"].IsNullOrEmpty())
				return;

			if (record["id"].IsNullOrEmpty() && record["transaction_type"].IsNullOrEmpty())
				throw new Exception("TransactionType not found!");

			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
				{
					_picklistRepository.CurrentUser = _currentUser;
					var transactionType = await _picklistRepository.GetItemById((int)record["transaction_type"]);

					if (transactionType == null)
						throw new Exception("TransactionType picklist item not found!");

					record["transaction_type_system"] = transactionType.Value;
				}
			}
		}

		public async Task SetForecastFields(JObject record)
		{
			if (!record["stage"].IsNullOrEmpty())
			{
				using (var _scope = _serviceScopeFactory.CreateScope())
				{
					var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
					using (var _picklistRepository = new PicklistRepository(databaseContext, _configuration))
					{
						_picklistRepository.CurrentUser = _currentUser;
						var stage = await _picklistRepository.GetItemById((int)record["stage"]);

						if (stage == null)
							throw new Exception("Stage picklist not found!");

						record["forecast_type"] = stage.Value2;
						record["forecast_category"] = stage.Value3;
					}
				}
			}

			if (!record["closing_date"].IsNullOrEmpty())
			{
				var closingDate = (DateTime)record["closing_date"];

				record["forecast_year"] = closingDate.Year;
				record["forecast_month"] = closingDate.Month;
				record["forecast_quarter"] = closingDate.ToQuarter();
			}
		}

		public JObject CreateStageHistoryRecord(JObject record, JObject currentRecord)
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

		public string GetRecordPrimaryValue(JObject record, Module module)
		{
			var recordName = string.Empty;
			var primaryField = module.Fields.FirstOrDefault(x => x.Primary);

			if (primaryField != null)
				recordName = (string)record[primaryField.Name];

			return recordName;
		}

		public async Task<List<string>> GetAllFieldsForFindRequest(string moduleName, bool withLookups = true)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _moduleRepository = new ModuleRepository(databaseContext, _configuration))
				{
					_moduleRepository.CurrentUser = _currentUser;
					var module = await _moduleRepository.GetByNameBasic(moduleName);
					var fields = new List<string>();

					var platformUser = (PlatformUser)_context.HttpContext.Items["user"];
					var tenant = platformUser.TenantsAsUser.Single().Tenant;

					foreach (var field in module.Fields)
					{
						if (field.Deleted || field.LookupType == "relation")
							continue;

						if (field.DataType == DataType.Lookup && withLookups)
						{
							Module lookupModule;

							if (field.LookupType == "users")
								lookupModule = Model.Helpers.ModuleHelper.GetFakeUserModule();
							else if (field.LookupType == "profiles")
								lookupModule = Model.Helpers.ModuleHelper.GetFakeProfileModule();
							else if (field.LookupType == "roles")
								lookupModule = Model.Helpers.ModuleHelper.GetFakeRoleModule(tenant.Setting.Language);
							else
								lookupModule = await _moduleRepository.GetByName(field.LookupType);

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

		/// <summary>
		/// Sets current user in case if it's unavailable from context.
		/// </summary>
		/// <param name="currentUser"></param>
		public void SetCurrentUser(UserItem appUser)
		{
			_currentUser = new CurrentUser()
			{
				TenantId = appUser.TenantId,
				UserId = appUser.Id

			};
		}

		public void Dispose()
		{
		}
	}
}