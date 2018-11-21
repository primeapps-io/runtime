using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Notification;
using PrimeApps.Model.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace PrimeApps.App.Notifications
{
	public interface IActivityHelper
	{
		Task Create(UserItem appUser, JObject record, Module module, Warehouse warehouse, bool createForExisting = true, int timezoneOffset = 180);

		Task Event(UserItem appUser, JObject record, Module module, int timezoneOffset = 180);

		Task Call(UserItem appUser, JObject record, Module module, int timezoneOffset = 180);

		Task Task(UserItem appUser, JObject record, Module module, bool createForExisting = true, int timezoneOffset = 180);

		Task Update(UserItem appUser, JObject record, JObject currentRecord, Module module, Warehouse warehouse, int timezoneOffset = 180);

		Task EventUpdate(UserItem appUser, JObject record, Reminder reminderExisting, int timezoneOffset = 180);

		Task CallUpdate(UserItem appUser, JObject record, Reminder reminderExisting, int timezoneOffset = 180);
		Task Delete(UserItem appUser, JObject record, Module module, IConfiguration configuration, int timezoneOffset = 180);
	}
	public class ActivityHelper : IActivityHelper
	{
		private CurrentUser _currentUser;
		private IHttpContextAccessor _context;
		private IConfiguration _configuration;
		private IServiceScopeFactory _serviceScopeFactory;

		public ActivityHelper(IServiceScopeFactory serviceScopeFactory, IHttpContextAccessor context, IConfiguration configuration)
		{
			_context = context;
			_currentUser = UserHelper.GetCurrentUser(_context);
			_serviceScopeFactory = serviceScopeFactory;
			_configuration = configuration;
		}

        public ActivityHelper(IConfiguration configuration,IServiceScopeFactory serviceScopeFactory,CurrentUser currentUser)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;

            _currentUser = currentUser;
        }

        #region Create
        public async Task Create(UserItem appUser, JObject record, Module module, Warehouse warehouse, bool createForExisting = true, int timezoneOffset = 180)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                //Set warehouse database name
                warehouse.DatabaseName = appUser.WarehouseDatabaseName;

				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

				string activityType = record["activity_type_system"]?.ToString();

				if (!string.IsNullOrEmpty(activityType))
				{
					using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
					using (var recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
					{
						recordRepository.CurrentUser = moduleRepository.CurrentUser = _currentUser;
						var recordId = record["id"]?.ToString();

						var userModule = Model.Helpers.ModuleHelper.GetFakeUserModule();
						var listLookupModule = new List<Module>
								{
									userModule
								};

						var fullRecord = recordRepository.GetById(module, Convert.ToInt32(recordId), !appUser.HasAdminProfile, listLookupModule);

						recordRepository.SetPicklists(module, fullRecord, appUser.TenantLanguage);

						if (!fullRecord.IsNullOrEmpty())
						{

							switch (activityType)
							{
								case "task":
									await Task(appUser, fullRecord, module, createForExisting, timezoneOffset);
									break;
								case "event":
									await Event(appUser, fullRecord, module, timezoneOffset);
									break;
								case "call":
									await Call(appUser, fullRecord, module, timezoneOffset);
									break;
							}
						}
					}
				}
			}
		}

		/// <summary>
		/// Creates notifications for event typed activity records.
		/// </summary>
		/// <param name="cloudantClient"></param>
		/// <param name="appUser"></param>
		/// <param name="record"></param>
		/// <returns></returns>
		public async Task Event(UserItem appUser, JObject record, Module module, int timezoneOffset = 180)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _reminderRepository = new ReminderRepository(databaseContext, _configuration))
				{
					_reminderRepository.CurrentUser = _currentUser;
					if (record["event_reminder"].IsNullOrEmpty()) return;

					long remindBefore = (long)record["event_reminder"]["value"];
					DateTime eventEndDate = (DateTime)record["event_end_date"];
					DateTime eventStartDate = (DateTime)record["event_start_date"];
					DateTime nextReminder = eventStartDate.AddMinutes(-1 * remindBefore);

					/// set reminder end to the last minute of the day.
					eventEndDate = eventEndDate.AddHours(23).AddMinutes(59).AddSeconds(59);
					record["event_end_date"] = eventEndDate;

					Reminder reminder = new Reminder()
					{
						ModuleId = module.Id,
						ReminderScope = module.Name,
						RecordId = (int)record["id"],
						ReminderType = (string)record["activity_type"]["value"],
						ReminderFrequency = 0,
						ReminderStart = eventStartDate,
						ReminderEnd = eventEndDate,
						Subject = (string)record["subject"],
						Rev = Utils.CreateRandomString(20),
						Owner = (int?)record["owner.id"],
						TimeZoneOffset = timezoneOffset
					};

					var newReminder = await _reminderRepository.Create(reminder);
					if (newReminder != null)
					{
						/// create reminder object
						ReminderDTO reminderDto = new ReminderDTO();
						reminderDto.Id = newReminder.Id.ToString();
						reminderDto.Rev = newReminder.Rev;
						reminderDto.TenantId = appUser.TenantId;

						/// send message to the queue.
						//await ServiceBus.SendMessage("reminder", reminderDto, nextReminder);

						DateTimeOffset dateOffset = DateTime.SpecifyKind(nextReminder, DateTimeKind.Utc);
						Hangfire.BackgroundJob.Schedule<Jobs.Reminder.Activity>(activity => activity.Process(reminderDto, appUser), dateOffset);
					}
				}
			}
		}

		/// <summary>
		/// Creates notifications for call typed activity records.
		/// </summary>
		/// <returns></returns>
		public async Task Call(UserItem appUser, JObject record, Module module, int timezoneOffset = 180)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _reminderRepository = new ReminderRepository(databaseContext, _configuration))
				{
					_reminderRepository.CurrentUser = _currentUser;
					if (record["event_reminder"].IsNullOrEmpty()) return;

					long remindBefore = (long)record["event_reminder"]["value"];
					DateTime callEndDate = (DateTime)record["call_time"];
					DateTime callStartDate = (DateTime)record["call_time"];
					DateTime nextReminder = callStartDate.AddMinutes(-1 * remindBefore);

					Reminder reminder = new Reminder()
					{
						RecordId = (int)record["id"],
						ModuleId = module.Id,
						ReminderScope = module.Name,
						ReminderType = (string)record["activity_type"]["value"],
						ReminderFrequency = 0,
						ReminderStart = callStartDate,
						ReminderEnd = callEndDate,
						Subject = (string)record["subject"],
						Rev = Utils.CreateRandomString(20),
						Owner = (int?)record["owner.id"],
						TimeZoneOffset = timezoneOffset
					};

					var newReminder = await _reminderRepository.Create(reminder);
					if (newReminder != null)
					{
						/// create reminder object
						ReminderDTO reminderDto = new ReminderDTO();
						reminderDto.Id = newReminder.Id.ToString();
						reminderDto.Rev = newReminder.Rev;
						reminderDto.TenantId = appUser.TenantId;

						/// send message to the queue.
						//await ServiceBus.SendMessage("reminder", reminderDto, nextReminder);
						DateTimeOffset dateOffset = DateTime.SpecifyKind(nextReminder, DateTimeKind.Utc);

						Hangfire.BackgroundJob.Schedule<Jobs.Reminder.Activity>(activity => activity.Process(reminderDto, appUser), dateOffset);

					}
				}
			}
		}

		/// <summary>
		/// Creates notifications for task typed activity records.
		/// </summary>
		/// <param name="cloudantClient"></param>
		/// <param name="appUser"></param>
		/// <param name="record"></param>
		/// <param name="createForExisting">if the record is being updated, but there is no previous notification defined.</param>
		/// <returns></returns>
		public async Task Task(UserItem appUser, JObject record, Module module, bool createForExisting = true, int timezoneOffset = 180)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _reminderRepository = new ReminderRepository(databaseContext, _configuration))
				{
					_reminderRepository.CurrentUser = _currentUser;
					if (!record["task_due_date"].IsNullOrEmpty() && !record["task_reminder"].IsNullOrEmpty() && !(!record["task_status"].IsNullOrEmpty() && (string)record["task_status"]["value"] == "completed"))
					{
						DateTime now = DateTime.UtcNow;
						DateTime taskDueDate = (DateTime)record["task_due_date"];
						DateTime taskReminderDate = (DateTime)record["task_reminder"];

						/// set reminder end to the last minute of the day.
						taskDueDate = taskDueDate.AddHours(23).AddMinutes(59).AddSeconds(59);

						Reminder reminder = new Reminder()
						{
							ModuleId = module.Id,
							ReminderScope = module.Name,
							RecordId = (int)record["id"],
							ReminderEnd = (DateTime)record["task_due_date"],
							ReminderStart = (DateTime)record["task_reminder"],
							ReminderFrequency = !record["reminder_recurrence"].IsNullOrEmpty() ? (int?)record["reminder_recurrence"]["value"] : null,
							ReminderType = (string)record["activity_type"]["value"],
							Subject = (string)record["subject"],
							Rev = Utils.CreateRandomString(20),
							Owner = (int?)record["owner.id"],
							TimeZoneOffset = timezoneOffset

						};
						var newReminder = await _reminderRepository.Create(reminder);
						if (newReminder != null)
						{
							/// Cannot create a reminder message for a date after the deadline of the task.
							if (taskDueDate > taskReminderDate || taskDueDate >= now)
							{
								/// create reminder object
								ReminderDTO reminderDto = new ReminderDTO();
								reminderDto.Id = newReminder.Id.ToString();
								reminderDto.Rev = reminder.Rev;
								reminderDto.TenantId = appUser.TenantId;

								DateTimeOffset dateOffset = DateTime.SpecifyKind(taskReminderDate, DateTimeKind.Utc);
								/// send message to the queue.
								Hangfire.BackgroundJob.Schedule<Jobs.Reminder.Activity>(activity => activity.Process(reminderDto, appUser), dateOffset);
							}
						}
					}
				}
			}
		}
		#endregion

		#region Update
		/// <summary>
		/// Notifications to be updated will be sent this method 
		/// </summary>
		/// <param name="appUser"></param>
		/// <param name="record"></param>
		/// <param name="module"></param>
		/// <returns></returns>
		public async Task Update(UserItem appUser, JObject record, JObject currentRecord, Module module, Warehouse warehouse, int timezoneOffset = 180)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				//Set warehouse database name
				warehouse.DatabaseName = appUser.WarehouseDatabaseName;

				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _reminderRepository = new ReminderRepository(databaseContext, _configuration))
				using (var _recordRepository = new RecordRepository(databaseContext, warehouse, _configuration))
				{
					_reminderRepository.CurrentUser = _recordRepository.CurrentUser = _currentUser;
					var recordId = record["id"].ToString();
					var reminderExisting = await _reminderRepository.GetReminder(Convert.ToInt32(recordId), null, module.Id);
					if (reminderExisting == null)
					{
						/// no exiting notification for the activity. redirect it to create method.
						switch (module.Name)
						{
							case "activities":
								var userModule = Model.Helpers.ModuleHelper.GetFakeUserModule();
								var listLookupModule = new List<Module>();
								listLookupModule.Add(userModule);
								var databaseRecord = _recordRepository.GetById(module, Convert.ToInt32(recordId), !appUser.HasAdminProfile, listLookupModule);
								_recordRepository.SetPicklists(module, databaseRecord, appUser.TenantLanguage);

								//create case activity system type needed
								databaseRecord["activity_type_system"] = databaseRecord["activity_type"]?["value"]?.ToString();

								await (Create(appUser, databaseRecord, module, warehouse, true, timezoneOffset: timezoneOffset));
								break;
								//case "opportunities"://check here if any need on that

								//    break;
						}

						return;
					}
					else
					{
						/// there is an existing notification for this activity record. so just fetch it and send to the respective methods to update.
						_recordRepository.TenantId = appUser.TenantId;
						_recordRepository.UserId = appUser.TenantId;

						var userModule = Model.Helpers.ModuleHelper.GetFakeUserModule();
						var listLookupModule = new List<Module>();
						listLookupModule.Add(userModule);
						var fullRecord = _recordRepository.GetById(module, Convert.ToInt32(recordId), !appUser.HasAdminProfile, listLookupModule);

						_recordRepository.SetPicklists(module, fullRecord, appUser.TenantLanguage);

						string activityType = fullRecord["activity_type"]?["value"]?.ToString();

						switch (activityType)
						{
							case "task":
								await TaskUpdate(appUser, fullRecord, reminderExisting, timezoneOffset);
								break;
							case "event":
								await EventUpdate(appUser, fullRecord, reminderExisting, timezoneOffset);
								break;
							case "call":
								await CallUpdate(appUser, fullRecord, reminderExisting, timezoneOffset);
								break;
							default:
								break;
						}
					}
				}
			}
		}

		/// <summary>
		/// Updates event typed notifications.
		/// </summary>
		/// <param name="cloudantClient"></param>
		/// <param name="appUser"></param>
		/// <param name="record"></param>
		/// <param name="eventNotification"></param>
		/// <returns></returns>
		public async Task EventUpdate(UserItem appUser, JObject record, Reminder reminderExisting, int timezoneOffset = 180)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _reminderRepository = new ReminderRepository(databaseContext, _configuration))
				{
					_reminderRepository.CurrentUser = _currentUser;
					if (record["event_reminder"].IsNullOrEmpty())
					{
						await _reminderRepository.Delete((int)reminderExisting.RecordId, (int)reminderExisting.ModuleId);
						return;
					}

					long remindBefore = (long)record["event_reminder"]["value"];
					DateTime eventEndDate = (DateTime)record["event_end_date"];
					DateTime eventStartDate = (DateTime)record["event_start_date"];
					DateTime remindOn = eventStartDate.AddMinutes(-1 * remindBefore);
					DateTime now = DateTime.UtcNow;

					if (remindOn < now) return;


					reminderExisting.ReminderStart = eventStartDate;
					reminderExisting.ReminderEnd = eventEndDate;
					reminderExisting.Subject = (string)record["subject"];
					reminderExisting.Owner = (int?)record["owner.id"];
					reminderExisting.TimeZoneOffset = timezoneOffset;


					var updatedReminder = await _reminderRepository.Update(reminderExisting);
					if (updatedReminder != null)
					{
						/// create reminder object
						ReminderDTO reminder = new ReminderDTO();
						reminder.Id = updatedReminder.Id.ToString();
						reminder.Rev = updatedReminder.Rev;
						reminder.TenantId = appUser.TenantId;

						/// send message to the queue.
						//await ServiceBus.SendMessage("reminder", reminder, remindOn);
						DateTimeOffset dateOffset = DateTime.SpecifyKind(remindOn, DateTimeKind.Utc);
						Hangfire.BackgroundJob.Schedule<Jobs.Reminder.Activity>(activity => activity.Process(reminder, appUser), dateOffset);
					}
				}
			}

		}

		/// <summary>
		/// Updates call typed notifications.
		/// </summary>
		public async Task CallUpdate(UserItem appUser, JObject record, Reminder reminderExisting, int timezoneOffset = 180)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _reminderRepository = new ReminderRepository(databaseContext, _configuration))
				{
					_reminderRepository.CurrentUser = _currentUser;
					if (record["call_time"].IsNullOrEmpty())
					{
						/// user removed call reminder, make it so.
						await _reminderRepository.Delete((int)reminderExisting.RecordId, (int)reminderExisting.ModuleId);
						return;
					}

					DateTime callStartDate = (DateTime)record["call_time"];
					DateTime now = DateTime.UtcNow;

					if (callStartDate < now) return;
					reminderExisting.ReminderStart = callStartDate;
					reminderExisting.ReminderEnd = callStartDate;//for call set same value
					reminderExisting.Subject = (string)record["subject"];
					reminderExisting.Owner = (int?)record["owner.id"];
					reminderExisting.TimeZoneOffset = timezoneOffset;

					var updatedReminder = await _reminderRepository.Update(reminderExisting);
					if (updatedReminder != null)
					{
						/// create reminder object
						ReminderDTO reminderDto = new ReminderDTO();
						reminderDto.Id = reminderExisting.Id.ToString();
						reminderDto.Rev = updatedReminder.Rev;
						reminderDto.TenantId = appUser.TenantId;

						/// send message to the queue.
						//await ServiceBus.SendMessage("reminder", reminderDto, callStartDate);
						DateTimeOffset dateOffset = DateTime.SpecifyKind(callStartDate, DateTimeKind.Utc);
						Hangfire.BackgroundJob.Schedule<Jobs.Reminder.Activity>(activity => activity.Process(reminderDto, appUser), dateOffset);
					}
				}
			}
		}

		/// <summary>
		/// Updates task typed notifications.
		/// </summary>
		/// <param name="cloudantClient"></param>
		/// <param name="appUser"></param>
		/// <param name="record"></param>
		/// <param name="taskNotification"></param>
		/// <returns></returns>
		public async Task TaskUpdate(UserItem appUser, JObject record, Reminder reminderExisting, int timezoneOffset = 180)
		{
			using (var _scope = _serviceScopeFactory.CreateScope())
			{
				var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _reminderRepository = new ReminderRepository(databaseContext, _configuration))
				{
					_reminderRepository.CurrentUser = _currentUser;
					if (!record["task_status"].IsNullOrEmpty() && (string)record["task_status"]["value"] == "completed")
					{
						/// even user completed task, we should also remove it.
						await _reminderRepository.Delete((int)reminderExisting.RecordId, (int)reminderExisting.ModuleId);
						return;
					}

					DateTime now = DateTime.UtcNow;
					DateTime taskDueDate = (DateTime)record["task_due_date"];
					DateTime taskReminderStartDate = (DateTime)record["task_reminder"];
					DateTime remindedOn = taskReminderStartDate;
					DateTime? remindOn = taskReminderStartDate;

					int reminderFrequency = 0;

                    /// If task due date and reminder start date passed from now datetime, task is already outdated.
                    if (taskDueDate < now && taskReminderStartDate < now) return;

                    /// set reminder end to the last minute of the day.
                    taskDueDate = taskDueDate.AddHours(23).AddMinutes(59).AddSeconds(59);


					reminderExisting.ReminderStart = (DateTime)record["task_reminder"];//tarihleri beni kıllandırıyor kontrol edelim.
					reminderExisting.ReminderEnd = (DateTime)record["task_due_date"];
					reminderExisting.Subject = (string)record["subject"];
					reminderExisting.Owner = (int?)record["owner.id"];
					reminderExisting.TimeZoneOffset = timezoneOffset;

					if (!record["reminder_recurrence"].IsNullOrEmpty())
					{
						reminderExisting.ReminderFrequency = (int?)record["reminder_recurrence"]["value"];
						reminderFrequency = (int)record["reminder_recurrence"]["value"];
					}
					else
					{
						/// there will be no reminders for this task.
						reminderExisting.ReminderFrequency = null;
					}


					if (reminderExisting.RemindedOn != null)
					{
						/// this task has already been reminded.
						remindedOn = (DateTime)reminderExisting.RemindedOn;
					}

					if (remindedOn > taskReminderStartDate)
					{
						/// reminder start date is smaller then the last reminder.
						remindOn = remindedOn.AddMinutes(reminderFrequency);
					}
					else
					{
						/// task reminder start date has changed to a further date.
						remindOn = taskReminderStartDate;
					}


					var updatedReminder = await _reminderRepository.Update(reminderExisting);
					if (updatedReminder == null) return;


					/// create reminder object
					ReminderDTO reminder = new ReminderDTO();
					reminder.Id = updatedReminder.Id.ToString();
					reminder.Rev = reminderExisting.Rev;
					reminder.TenantId = appUser.TenantId;

					/// send message to the queue.
					//await ServiceBus.SendMessage("reminder", reminder, (DateTime)remindOn);
					DateTimeOffset dateOffset = DateTime.SpecifyKind((DateTime)remindOn, DateTimeKind.Utc);

					Hangfire.BackgroundJob.Schedule<Jobs.Reminder.Activity>(activity => activity.Process(reminder, appUser), dateOffset);
				}
			}
		}
		#endregion

		#region Delete
		public async Task Delete(UserItem appUser, JObject record, Module module, IConfiguration configuration, int timezoneOffset = 180)
		{
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				using (var _reminderRepository = new ReminderRepository(tenantDatabaseContext, configuration))
				{
					_reminderRepository.CurrentUser = _currentUser;
					int recordId = (int)record["id"];
					await _reminderRepository.Delete(recordId, module.Id);
				}
			}
		}

	}
	#endregion
}



