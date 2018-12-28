using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Notification;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs.Reminder
{
    public class Activity
    {
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public Activity(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        /// <summary>
        /// Processes reminder object and classifies the type of the reminder.
        /// </summary>
        /// <param name="reminderMessage"></param>
        /// <returns>it will return false only when an error occured during the processing, not when the reminder revision is wrong or non-existant.</returns>
        [QueueCustom]
        public async Task<bool> Process(ReminderDTO reminderMessage, UserItem appUser)
        {
            Model.Entities.Tenant.Reminder reminder;
            bool status = false;

            try
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var databaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                    var platformDatabaseContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                    var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                    databaseContext.TenantId = reminderMessage.TenantId;

                    using (var reminderRepository = new ReminderRepository(databaseContext, _configuration))
                    using (var userRepository = new UserRepository(databaseContext, _configuration))
                    {
                        reminderRepository.CurrentUser = userRepository.CurrentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };
                        /// Get related reminder record from data store.
                        reminder = await reminderRepository.GetById(Convert.ToInt32(reminderMessage.Id));

                        if (reminder == null) return true;

                        /// check if the message is still valid.
                        if (reminder.Rev != reminderMessage.Rev) return true;

                        string reminderType = reminder.ReminderType;

                        using (PlatformUserRepository platformUserRepository = new PlatformUserRepository(platformDatabaseContext, _configuration, cacheHelper))
                        {
                            platformUserRepository.CurrentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };

                            try
                            {
                                switch (reminderType)
                                {
                                    case "task":
                                        await Task(reminder, reminderMessage, userRepository, platformUserRepository, reminderRepository, appUser, _configuration);
                                        break;
                                    case "event":
                                        await Event(reminder, reminderMessage, userRepository, platformUserRepository, appUser);
                                        break;
                                    case "call":
                                        await Call(reminder, reminderMessage, userRepository, platformUserRepository, appUser);
                                        break;
                                    default:
                                        break;
                                }
                                status = true;
                            }
                            catch (Exception ex)
                            {
                                /// rollback the transaction and log error.
                                ErrorHandler.LogError(ex, "Error while processing activity notification.");
                                status = false;
                            }

                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, $"Reminder Process has failed while running id: {reminderMessage.Id} rev: {reminderMessage.Rev} tenant: {reminderMessage.TenantId}.");
                status = false;
            }

            return status;
        }

        /// <summary>
        /// Creates notifications for event typed activity records.
        /// </summary>
        private async Task Event(Model.Entities.Tenant.Reminder reminder, ReminderDTO reminderMessage, IUserRepository userRepository, PlatformUserRepository platformUserRepository, UserItem _appUser)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime reminderStart = reminder.ReminderStart;
                DateTime eventEnd = reminder.ReminderEnd;

                var usr = await userRepository.GetById((int)reminder.Owner);

                string email = usr.Email;
                string subject = reminder.Subject;

                string userName = string.Format("{0} {1}", usr.FirstName, usr.LastName),
                startDate = reminderStart.AddMinutes(reminder.TimeZoneOffset).ToString("dd.MM.yyyy HH:mm"),
                endDate = eventEnd.AddMinutes(reminder.TimeZoneOffset).ToString("dd.MM.yyyy HH:mm");

                var user = await platformUserRepository.Get(usr.Email);
                var appUser = new UserItem
                {
                    AppId = _appUser.AppId,
                    TenantId = _appUser.TenantId,
                    Id = user.Id,
                    Email = user.Email
                };

                Email.Notification.Event(userName, subject, email, usr.Culture, startDate, endDate, _appUser.AppId, appUser, _configuration, _serviceScopeFactory);
                /// program notification email for the reminder date.
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, $"Reminder Event has failed while running id: {reminderMessage.Id} rev: {reminderMessage.Rev} tenant: {reminderMessage.TenantId}.");
            }
        }

        /// <summary>
        /// Creates notifications for call typed activity records.
        /// </summary>
        private async Task Call(Model.Entities.Tenant.Reminder reminder, ReminderDTO reminderMessage, IUserRepository userRepository, PlatformUserRepository platformUserRepository, UserItem _appUser)
        {
            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime reminderStart = reminder.ReminderStart;
                DateTime eventEnd = reminder.ReminderEnd;

                var usr = await userRepository.GetById((int)reminder.Owner);

                string email = usr.Email;
                string subject = reminder.Subject;

                string userName = string.Format("{0} {1}", usr.FirstName, usr.LastName),
                    startDate = reminderStart.AddMinutes(reminder.TimeZoneOffset).ToString("dd.MM.yyyy HH:mm");

                var user = await platformUserRepository.Get(usr.Email);
                var appUser = new UserItem
                {
                    AppId = _appUser.AppId,
                    TenantId = _appUser.TenantId,
                    Id = user.Id,
                    Email = user.Email
                };

                Email.Notification.Call(userName, subject, email, usr.Culture, startDate, _appUser.AppId, appUser, _configuration, _serviceScopeFactory);
                /// program notification email for the reminder date.
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, $"Reminder Call has failed while running id: {reminderMessage.Id} rev: {reminderMessage.Rev} tenant: {reminderMessage.TenantId}.");
            }
        }

        /// <summary>
        /// Creates notifications for task typed activity records.
        /// </summary>
        private async Task Task(Model.Entities.Tenant.Reminder reminder, ReminderDTO reminderMessage, IUserRepository userRepository, PlatformUserRepository platformUserRepository, ReminderRepository reminderRepository, UserItem _appUser, IConfiguration configuration)
        {

            try
            {
                DateTime now = DateTime.UtcNow;
                DateTime reminderStart = reminder.ReminderStart;
                DateTime reminderEnd = reminder.ReminderEnd;
                DateTime remindOn = now;
                long reminderFrequency = 0;

                if (reminder.ReminderFrequency != null)
                {
                    reminderFrequency = (long)reminder.ReminderFrequency;
                }

                var usr = await userRepository.GetById((int)reminder.Owner);

                string email = usr.Email;
                string subject = reminder.Subject;
                string deadline = reminderEnd.AddMinutes(reminder.TimeZoneOffset).ToString("dd.MM.yyyy");

                string userName = string.Format("{0} {1}", usr.FirstName, usr.LastName);

                var user = await platformUserRepository.Get(usr.Email);

                /// send notification email.
                var appUser = new UserItem
                {
                    AppId = _appUser.AppId,
                    TenantId = _appUser.TenantId,
                    Id = user.Id,
                    Email = user.Email
                };

                Email.Notification.Task(userName, subject, email, usr.Culture, deadline, _appUser.AppId, appUser, _configuration, _serviceScopeFactory);

                while (remindOn <= now && reminderFrequency != 0)
                {
                    /// safety mechanism to prevent reminder message flood to the user.
                    remindOn = remindOn.AddMinutes(reminderFrequency);
                }

                if (reminderFrequency != 0 && (reminderEnd >= remindOn))
                {
                    /// reminders are periodic and the next reminder date is before or at the same time with the deadline.

                    /// update remind_on property for the next time.               
                    reminder.RemindedOn = remindOn;

                    //dynamic result = await cloudantClient.UpdateAsync((string)record._id, record);
                    var result = await reminderRepository.Update(reminder);
                    if (result != null)
                    {
                        reminderMessage.Rev = result.Rev;
                        DateTimeOffset dateOffset = DateTime.SpecifyKind(remindOn, DateTimeKind.Utc);
                        Hangfire.BackgroundJob.Schedule<Jobs.Reminder.Activity>(activity => activity.Process(reminderMessage, _appUser), dateOffset);

                    }
                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, $"Reminder Task has failed while running id: {reminderMessage.Id} rev: {reminderMessage.Rev} tenant: {reminderMessage.TenantId}.");
            }
        }
    }
}
