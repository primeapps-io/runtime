using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PrimeApps.Model.Repositories;
using PrimeApps.App.Jobs.Messaging.SMS.Providers;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Messaging;
using PrimeApps.Model.Common.Record;
using RecordHelper = PrimeApps.Model.Helpers.RecordHelper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.App.Jobs.Messaging.SMS
{
    /// <summary>
    /// Sends bulk sms messages via choosen sms provider.
    /// </summary>
    public class SMSClient : MessageClient
    {
        private IConfiguration _configuration;

        public SMSClient(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Processes bulk sms request, prepares and sends it.
        /// </summary>
        /// <param name="smsQueueItem"></param>
        /// <returns></returns>
        public override async Task<bool> Process(MessageDTO smsQueueItem)
        {

            string[] ids;
            bool isAllSelected = false;
            string messageTemplate = "",
                query = "",
                moduleId = "",
                language = "",
                smsId = "",
                smsRev = "",
                owner = "",
                moduleName = "",
                phoneField = "";
            DateTime queueDate = DateTime.UtcNow;

            SMSComposerResult composerResult = new SMSComposerResult();
            NotificationStatus bulkSMSStatus = NotificationStatus.Successful;
            SMSResponse smsResponse = new SMSResponse();
            IList<dynamic> messageStatuses = new List<dynamic>();

            try
            {

				using (var dbContext = new TenantDBContext(smsQueueItem.TenantId, _configuration))
                {
                    /// get sms settings.
                    var smsSettings = dbContext.Settings.Include(x => x.CreatedBy).Where(r => r.Type == Model.Enums.SettingType.SMS && r.Deleted == false).ToList();
                    /// email settings are null just return and do nothing.
                    if (smsSettings == null)
                    {
                        bulkSMSStatus = NotificationStatus.InvalidProvider;
                    }

                    var provider = smsSettings.FirstOrDefault(r => r.Key == "provider")?.Value;
                    var userName = smsSettings.FirstOrDefault(r => r.Key == "user_name")?.Value;
                    var password = smsSettings.FirstOrDefault(r => r.Key == "password")?.Value;
                    var alias = smsSettings.FirstOrDefault(r => r.Key == "alias")?.Value;


                    if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(alias))
                    {
                        bulkSMSStatus = NotificationStatus.InvalidProvider;
                    }



                    using (SMSProvider smsClient = SMSProvider.Initialize(provider, userName, password))
                    {
                        smsClient.Alias = alias;

                        var notificationId = Convert.ToInt32(smsQueueItem.Id);
                        /// get details of the sms queue item.
                        var smsNotification = dbContext.Notifications.Include(x => x.CreatedBy).Where(r => r.NotificationType == NotificationType.Sms && r.Id == notificationId && r.Deleted == false).FirstOrDefault();

                        /// this request has already been removed, do nothing and return success.
                        if (smsNotification == null) return true;

                        //is selected all and its query..
                        ids = null;
                        if (smsNotification.Ids != "ALL") //If all will be queried at composeprepare method with filter query
                        {
                            ids = smsNotification.Ids.Split(new char[] { ',' }, options: StringSplitOptions.RemoveEmptyEntries);
                        }
                        else
                        {
                            isAllSelected = true;
                        }

                        query = smsNotification.Query;
                        messageTemplate = smsNotification.Template;
                        moduleId = smsNotification.ModuleId.ToString();
                        language = smsNotification.Lang;
                        smsId = smsNotification.Id.ToString();
                        smsRev = smsNotification.Rev;
                        owner = smsNotification.CreatedBy.Email;
                        phoneField = smsNotification.PhoneField;
                        Module module;

                        /// revisions are different, that means this record has already been processed.
                        if (smsQueueItem.Rev != smsRev) return true;

                        ///get related module
                        using (var moduleRepository = new ModuleRepository(dbContext, _configuration))
                        {
                            module = await moduleRepository.GetById(smsNotification.ModuleId);
                        }

                        moduleName = smsNotification.Lang == "en" ? module.LabelEnSingular : module.LabelTrSingular;

                        if (module == null) return true;

                        /// process and send messages only if the provider is valid.
                        if (bulkSMSStatus == NotificationStatus.Successful)
                        {
                            /// compose bulk messages for sending.
                            composerResult = await Compose(smsQueueItem, messageTemplate, module, phoneField, query, ids, isAllSelected, smsNotification.CreatedById, language, smsId, dbContext, smsClient);

                            /// send composed messages through selected provider.
                            smsResponse = await smsClient.Send(composerResult.Messages);

                            /// set main status to the response status.
                            bulkSMSStatus = smsResponse.Status;
                        }


                        smsNotification.Status = bulkSMSStatus;
                        composerResult.ProviderResponse = smsResponse.Status.ToString();
                        dbContext.Entry(smsNotification).State = EntityState.Modified;
                        smsNotification.Result = JsonConvert.SerializeObject(composerResult);
                        dbContext.SaveChanges();

                        ///Cleanup
                        composerResult.DetailedMessageStatusList.Clear();
                        composerResult.Messages.Clear();

                    }

                }
            }
            catch (Exception ex)
            {
                ErrorHandler.LogError(ex, $"SMS Client has failed while sending a short message template with id:{smsId} of tenant: {smsQueueItem.TenantId}.");
                bulkSMSStatus = NotificationStatus.SystemError;
            }

            //Email.Messaging.SendSMSStatusNotification(owner, messageTemplate, moduleName, queueDate, bulkSMSStatus, composerResult.Successful, composerResult.NotAllowed, composerResult.InvalidNumbers, composerResult.MissingNumbers);

            /// always return true to say queue that the job has done.
            return true;
        }

        public async Task<SMSComposerResult> Compose(MessageDTO messageDto, string messageBody, Module module, string phoneField, string query, string[] ids, bool isAllSelected, int userId, string language, string smsId, TenantDBContext dbContext, SMSProvider smsClient)
        {
            /// create required parameters for composing.
            Regex templatePattern = new Regex(@"{(.*?)}");
            IList<Message> messages = new List<Message>();
            IList<string> messageFields = new List<string>();
            JArray messageStatusList = new JArray();
            string queryFields = phoneField;
            int successful = 0,
                invalidNumbers = 0,
                missingNumbers = 0,
                noAddress = 0,
                notAllowed = 0;

			Tenant subscriber = null;
            using (PlatformDBContext platformDBContext = new PlatformDBContext(_configuration))
            {
                using (TenantRepository platformUserRepository = new TenantRepository(platformDBContext, _configuration))
                {

                    subscriber = await platformUserRepository.GetWithSettingsAsync(messageDto.TenantId);

                }
            }

			string culture = subscriber.Setting.Culture;
			string lang = subscriber.Setting.Language;

			if (!subscriber.App.UseTenantSettings)
			{
				culture = subscriber.App.Setting.Culture;
				lang = subscriber.App.Setting.Language;
			}

            /// get all required fields from template.
            MatchCollection matches = templatePattern.Matches(messageBody);

            foreach (object match in matches)
            {
                string fieldName = match.ToString().Replace("{", "").Replace("}", "");

                if (!messageFields.Contains(fieldName))
                {
                    messageFields.Add(fieldName);
                }
            }

            if (ids?.Length > 0 || isAllSelected)
            {
                var idsList = "";

                if (isAllSelected)
                {
                    //Query with filtered or non filtered selectedAll ids..
                    FindRequest findRequest = JsonConvert.DeserializeObject<FindRequest>(query);

                    //Set find request limit to our maximum value 3000 unlike filter default
                    using (var databaseContext = new TenantDBContext(messageDto.TenantId, _configuration))
                    {
                        using (var recordRepository = new RecordRepository(databaseContext, _configuration))
                        {
                            recordRepository.UserId = userId;

                            var records = recordRepository.Find(module.Name, findRequest, false);

                            if (records.Count > 0)
                            {
                                var idListSelectAll = new List<string>();

                                foreach (JObject record in records)
                                {
                                    idListSelectAll.Add(record["id"]?.ToString());
                                }

                                ids = idListSelectAll.ToArray();
                            }
                        }
                    }
                }

                if (ids?.Length > 0)
                {
                    using (var databaseContext = new TenantDBContext(messageDto.TenantId, _configuration))
                    {
                        using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
                        {
                            using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
                            {
                                using (var recordRepository = new RecordRepository(databaseContext, _configuration))
                                {
                                    foreach (string recordId in ids)
                                    {
                                        var status = MessageStatusEnum.Successful;
                                        string phoneNumber = "";
                                        var lookupModules = await RecordHelper.GetLookupModules(module, moduleRepository);
                                        var record = recordRepository.GetById(module, int.Parse(recordId), false, lookupModules);
                                        var recordCopy = record;
                                        record = await Model.Helpers.RecordHelper.FormatRecordValues(module, record, moduleRepository, picklistRepository, _configuration, lang, culture, 180, lookupModules);

                                        if (record[phoneField] != null)
                                        {
                                            /// don't send an sms if the mobile number is incompatible with the selected provider.
                                            phoneNumber = record[phoneField]?.ToString();
                                            phoneNumber = smsClient.ParsePhoneNumber(phoneNumber);

                                            if (phoneNumber == null)
                                            {
                                                status = MessageStatusEnum.MissingField;
                                                missingNumbers++;
                                            }
                                            else if (phoneNumber == string.Empty)
                                            {
                                                status = MessageStatusEnum.InvalidField;
                                                invalidNumbers++;
                                            }
                                            else
                                            {
                                                status = MessageStatusEnum.Successful;
                                            }
                                        }
                                        else
                                        {
                                            status = MessageStatusEnum.MissingField;
                                            noAddress++;
                                        }
                                        if (!recordCopy["sms_opt_out"].IsNullOrEmpty() && status == MessageStatusEnum.Successful)
                                        {
                                            var isAllowedSms = (bool)recordCopy["sms_opt_out"];

                                            if (isAllowedSms)
                                            {
                                                status = MessageStatusEnum.OptedOut;
                                                notAllowed++;
                                            }
                                            else
                                            {
                                                status = MessageStatusEnum.Successful;
                                            }
                                        }

                                        string formattedMessage = FormatMessage(messageFields, messageBody, record);

                                        JObject messageStatus = new JObject();

                                        messageStatus["number"] = phoneNumber;
                                        messageStatus["message"] = formattedMessage;
                                        messageStatus["status"] = status.ToString();
                                        messageStatus["sms_id"] = smsId;
                                        messageStatus["record_primary_value"] = record[phoneField]?.ToString();
                                        messageStatus["module_id"] = module.Id;
                                        messageStatus["type"] = "sms_detail";
                                        messageStatus["record_id"] = record["id"]?.ToString();

                                        /// add status object to the status list.
                                        messageStatusList.Add(messageStatus);

                                        if (status == MessageStatusEnum.Successful)
                                        {
                                            /// create a message object and add it to the list.
                                            Message smsMessage = new Message();
                                            smsMessage.Recipients.Add(phoneNumber);
                                            smsMessage.Body = formattedMessage;
                                            messages.Add(smsMessage);
                                            successful++;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return new SMSComposerResult()
            {
                NotAllowed = notAllowed,
                Successful = successful,
                MissingNumbers = missingNumbers,
                Messages = messages,
                DetailedMessageStatusList = messageStatusList
            };
        }
    }
}
