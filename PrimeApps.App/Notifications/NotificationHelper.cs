using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Entities.Platform.Identity;

namespace PrimeApps.App.Notifications
{
    public static class NotificationHelper
    {
        #region Create
        /// <summary>
        /// Creates all necessary notifications for the record.
        /// </summary>
        /// <param name="appUser"></param>
        /// <param name="record"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static async Task Create(UserItem appUser, JObject record, Module module, int timezoneOffset)
        {
            string moduleName = module?.Name?.ToLower();

            switch (moduleName)
            {
                case "activities":
                    await Activity.Create(appUser, record, module, timezoneOffset: timezoneOffset);
                    break;
            }
        }
        #endregion

        #region Update

        /// <summary>
        /// Updates notifications by their type.
        /// </summary>
        /// <param name="appUser"></param>
        /// <param name="record"></param>
        /// <param name="module"></param>
        /// <param name="currentRecord"></param>
        /// <returns></returns>
        public static async Task Update(UserItem appUser, JObject record, JObject currentRecord, Module module, int timeZoneOffset = 180)
        {
            string moduleName = module?.Name?.ToLower();

            switch (moduleName)
            {
                case "activities":
                    await Activity.Update(appUser, record, currentRecord, module, timezoneOffset: timeZoneOffset);
                    break;
            }

            /// check if owner of the record changed.
            await IsOwnerChanged(appUser, record, currentRecord, module);
        }

        /// <summary>
        /// Creates a notification when the owner is changed for an updated module record.
        /// </summary>
        /// <param name="httpClient"></param>
        /// <param name="appUser"></param>
        /// <param name="record"></param>
        /// <returns></returns>
        private static async Task IsOwnerChanged(UserItem appUser, JObject record, JObject oldRecord, Module module)
        {
            using (var databaseContext = new TenantDBContext(appUser.TenantId))
            {
                using (var settingRepository = new SettingRepository(databaseContext))
                {
                    var settings = await settingRepository.GetAllSettings();
                    if (settings.Any(setting => setting.Key == "send_owner_changed_notification" && setting.Value == "true"))
                    {
                        string newOwner = record["owner"]?.ToString(),
                        oldOwner = oldRecord["owner"]?.ToString(),
                        id = record["id"]?.ToString();

                        bool isTask = false;

                        /// not a valid or updated record.
                        if (newOwner == null || id == null) return;


                        /// if owner is same, do nothing.
                        if (oldOwner == newOwner) return;


                        //??WHAT IS THIS 
                        if (module.Name == "activities")
                        {/// exceptional condition for tasks.
                            if (module.SystemType == Model.Enums.SystemType.System)
                            {

                                string activityType = record["activity_type"]?.ToString();
                                if (activityType == "Görev" || activityType == "Task")
                                {
                                    isTask = activityType == "task";
                                }
                            }

                        }

                        if (isTask)
                        {
                            ///it is a task so just notify user about the task.
                            await OwnerChangedTask(appUser, record, oldRecord, module);
                            return;
                        }

                        await OwnerChangedDefault(appUser, record, oldRecord, module);
                    }
                }
            }
        }

        private static async Task OwnerChangedTask(UserItem appUser, JObject record, JObject oldRecord, Module module)
        {
            DateTime dueDate = UnixDate.GetDate((long)record["task_due_date"]);

            string newOwnerId = record["owner"]?.ToString(),
                oldOwnerId = oldRecord["owner"]?.ToString(),
                oldOwnerName = string.Empty,
                newOwnerName = string.Empty,
                formattedDueDate = dueDate.ToString("dd.MM.yyyy", new CultureInfo(appUser.Culture));

            using (PlatformDBContext dbContext = new PlatformDBContext())
            using (PlatformUserRepository userRepository = new PlatformUserRepository(dbContext))
            {
                PlatformUser newOwner = await userRepository.Get(Convert.ToInt32(newOwnerId)),
                             oldOwner = await userRepository.Get(Convert.ToInt32(oldOwnerId));


                newOwnerName = newOwner.GetFullName();
                oldOwnerName = oldOwner.GetFullName();

                Dictionary<string, string> emailData = new Dictionary<string, string>();
                emailData.Add("Owner", newOwnerName);
                emailData.Add("OldOwner", oldOwnerName);
                emailData.Add("DueDate", formattedDueDate);

                Email email = new Email(typeof(Resources.Email.TaskOwnerChangedNotification), appUser.Culture, emailData, appUser.AppId);
                email.AddRecipient(newOwner.Email);
                email.AddToQueue(appUser: appUser);
            }
        }

        private static async Task OwnerChangedDefault(UserItem appUser, JObject record, JObject oldRecord, Module module)
        {
            string newOwnerId = record["owner"]?.ToString(),
                oldOwnerId = oldRecord["owner"]?.ToString(),
                oldOwnerName = string.Empty,
                newOwnerName = string.Empty,
                moduleName = appUser.TenantLanguage == "tr" ? module.LabelTrSingular : appUser.TenantLanguage == "en" ? module.LabelEnSingular : module.LabelEnSingular;
            JObject moduleRecordPrimary;

            using (PlatformDBContext platformDBContext = new PlatformDBContext())
            using (PlatformUserRepository userRepository = new PlatformUserRepository(platformDBContext))
            using (var tenantDBContext = new TenantDBContext(appUser.TenantId))
            using (var recordRepository = new RecordRepository(tenantDBContext))

            {
                PlatformUser newOwner = await userRepository.Get(Convert.ToInt32(newOwnerId)),
                               oldOwner = await userRepository.Get(Convert.ToInt32(oldOwnerId));


                newOwnerName = newOwner.GetFullName();
                oldOwnerName = oldOwner.GetFullName();

                Dictionary<string, string> emailData = new Dictionary<string, string>();
                emailData.Add("Owner", newOwnerName);
                emailData.Add("OldOwner", oldOwnerName);
                emailData.Add("Module", moduleName);
                //emailData.Add("PrimaryValue", primaryValue);

                var recordId = record["id"]?.ToString();


                recordRepository.TenantId = appUser.TenantId;
                recordRepository.UserId = appUser.TenantId;

                moduleRecordPrimary = recordRepository.GetById(module, Convert.ToInt32(recordId), false, null);

                var modulePkey = module.Fields.Where(x => x.Primary == true).Select(v => v.Name).FirstOrDefault();


                emailData.Add("PrimaryValue", moduleRecordPrimary[modulePkey]?.ToString()); //?wtf to change

                Email email = new Email(typeof(Resources.Email.OwnerChangedNotification), appUser.Culture, emailData, appUser.AppId);
                email.AddRecipient(newOwner.Email);
                email.AddToQueue(appUser: appUser);
            }
        }

        #endregion

        #region Delete
        /// <summary>
        /// Deletes all notifications related to a module record according to its type.
        /// </summary>
        /// <param name="appUser"></param>
        /// <param name="record"></param>
        /// <param name="module"></param>
        /// <returns></returns>
        public static async Task Delete(UserItem appUser, JObject record, Module module)
        {
            string moduleName = module?.Name?.ToLower();

            switch (moduleName)
            {
                case "activities":
                    await Activity.Delete(appUser, record, module);
                    break;
            }
        }
        #endregion

        public static void SendTaskNotification(JObject record, UserItem appUser, Module module)
        {
            // Get full record and set picklists
            JObject fullRecord;

            using (var dbContext = new TenantDBContext(appUser.TenantId))
            {
                using (var recordRepository = new RecordRepository(dbContext))
                {
                    recordRepository.TenantId = appUser.TenantId;
                    recordRepository.UserId = appUser.TenantId;

                    var userModule = Model.Helpers.ModuleHelper.GetFakeUserModule();
                    var listLookupModule = new List<Module>
                    {
                        userModule
                    };

                    fullRecord = recordRepository.GetById(module, (int)record["id"], !appUser.HasAdminProfile, listLookupModule);

                    recordRepository.SetPicklists(module, fullRecord, appUser.TenantLanguage);
                }
            }

            // Send task notification if it is true
            if (!fullRecord["task_notification"].IsNullOrEmpty() && (bool)fullRecord["task_notification"]["value"])
            {
                if ((int)fullRecord["owner.id"] == appUser.Id)
                    return;

                // send a task assigned email, if it is required.
                var emailData = new Dictionary<string, string>();
                emailData.Add("UserName", (string)fullRecord["owner.email"]);
                emailData.Add("DueDate", ((DateTime)fullRecord["task_due_date"]).ToString(appUser.TenantLanguage == "tr" ? "dd.MM.yyyy" : "MM/dd/yyyy"));
                emailData.Add("Subject", (string)fullRecord["subject"]);

                var email = new Email(typeof(Resources.Email.TaskAssignedNotification), appUser.Culture, emailData, appUser.AppId);
                email.AddRecipient((string)fullRecord["owner.email"]);
                email.AddToQueue(appUser: appUser);
            }
        }
    }
}