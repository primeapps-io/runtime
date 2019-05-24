using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Jobs.Messaging.EMail.Providers;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Messaging;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RecordHelper = PrimeApps.Model.Helpers.RecordHelper;

namespace PrimeApps.App.Jobs.Messaging.EMail
{
	public class EMailClient : MessageClient
	{
		private IConfiguration _configuration;
		private IServiceScopeFactory _serviceScopeFactory;
		private IHttpContextFactory _context;
		public EMailClient(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IHttpContextFactory context)
		{
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
			_context = context;
		}

		/// <summary>
		/// Processes bulk email request, prepares and sends it.
		/// </summary>
		/// <param name="emailQueueItem"></param>
		/// <returns></returns>
		public override async Task<bool> Process(MessageDTO emailQueueItem, UserItem appUser)
		{
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

			string[] ids;
			bool isAllSelected = false;
			string emailTemplate = "",
				query = "",
				moduleId = "",
				language = "",
				emailId = "",
				emailRev = "",
				owner = "",
				moduleName = "",
				emailField = "",
				subject = "",
				senderAlias = "",
				senderEMail = "",
				Cc = "",
				Bcc = "";
			DateTime queueDate = DateTime.UtcNow;

			EMailComposerResult composerResult = new EMailComposerResult();
			NotificationStatus bulkEMailStatus = NotificationStatus.Successful;
			EMailResponse emailResponse = new EMailResponse();
			IList<dynamic> messageStatuses = new List<dynamic>();
			TenantUser emailOwner = new TenantUser();

			try
			{
				using (var scope = _serviceScopeFactory.CreateScope())
				{
					var databaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
					var platformDatabaseContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
					var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();

					databaseContext.TenantId = emailQueueItem.TenantId;

					using (var platformUserRepository = new PlatformUserRepository(platformDatabaseContext, _configuration))//, cacheHelper))
					using (var tenantRepository = new TenantRepository(platformDatabaseContext, _configuration))//, cacheHelper))
					using (var notifitionRepository = new NotificationRepository(databaseContext, _configuration))
					{

						notifitionRepository.CurrentUser = tenantRepository.CurrentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

                        /// get details of the email queue item.
                        ///
                        var notificationId = Convert.ToInt32(emailQueueItem.Id);

						//  var emailNotification = databaseContext.Notifications.Include(x => x.CreatedBy).FirstOrDefault(r => r.NotificationType == Model.Enums.NotificationType.Email && r.Id == notificationId && r.Deleted == false);

						var emailNotification = await notifitionRepository.GetById(notificationId);

						/// this request has already been removed, do nothing and return success.
						if (emailNotification == null) return true;

						var emailSet = await notifitionRepository.GetSetting(emailQueueItem, notificationId);
						// = databaseContext.Settings.Include(x => x.CreatedBy).Where(r =>
						//        r.Type == Model.Enums.SettingType.Email &&
						//        r.Deleted == false &&
						//        r.UserId == ((emailQueueItem.AccessLevel == AccessLevelEnum.Personal) ? (int?)emailNotification.CreatedById : null))
						//    .ToList();

						/// email settings are null just return and do nothing.
						if (emailSet == null)
						{
							bulkEMailStatus = NotificationStatus.InvalidProvider;
						}

						var provider = emailSet.FirstOrDefault(r => r.Key == "provider")?.Value;
						var userName = emailSet.FirstOrDefault(r => r.Key == "user_name")?.Value;
						var password = emailSet.FirstOrDefault(r => r.Key == "password")?.Value;
						var host = emailSet.FirstOrDefault(r => r.Key == "host")?.Value;
						var sslValue = emailSet.FirstOrDefault(r => r.Key == "enable_ssl")?.Value;
						var portValue = emailSet.FirstOrDefault(r => r.Key == "port")?.Value;

						bool sslEnabled = false;
						int port = 0;
						if (string.IsNullOrWhiteSpace(provider) || string.IsNullOrWhiteSpace(userName) || string.IsNullOrWhiteSpace(password) || string.IsNullOrWhiteSpace(sslValue) || string.IsNullOrWhiteSpace(portValue))
						{
							bulkEMailStatus = NotificationStatus.InvalidProvider;
						}
						else
						{
							sslEnabled = sslValue == "True" ? true : false;
							port = Convert.ToInt32(portValue);
						}

						using (EMailProvider emailClient = EMailProvider.Initialize(provider, userName, password))
						{
							emailClient.SetHost(host, port);

							/// enable secure connection if it is enabled by client config.
							emailClient.EnableSSL = sslEnabled;

							//is selected all and its query..
							ids = null;
							if (emailNotification.Ids != "ALL") //If all will be queried at composeprepare method with filter query
							{
								ids = emailNotification.Ids.Split(new char[] { ',' }, options: StringSplitOptions.RemoveEmptyEntries);
							}
							else
							{
								isAllSelected = true;
							}

							query = emailNotification.Query;
							emailTemplate = emailNotification.Template;
							moduleId = emailNotification.ModuleId.ToString();
							language = emailNotification.Lang;
							emailId = emailNotification.Id.ToString();
							emailRev = emailNotification.Rev;
							owner = emailNotification.CreatedBy.Email;
							emailOwner = emailNotification.CreatedBy;
							emailField = emailNotification.EmailField;
							subject = emailNotification.Subject;
							senderAlias = emailNotification.SenderAlias;
							senderEMail = emailNotification.SenderEmail;
							Cc = emailNotification.Cc;
							Bcc = emailNotification.Bcc;
							emailClient.SetSender(senderAlias, senderEMail);
							Module module;

							if (emailQueueItem.Rev != emailRev) return true;

							using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
							{
								moduleRepository.CurrentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };
                                module = await moduleRepository.GetById(emailNotification.ModuleId);
							}

							moduleName = emailNotification.Lang == "en" ? module.LabelEnSingular : module.LabelTrSingular;

							if (module == null) return true;

							if (bulkEMailStatus == NotificationStatus.Successful)
							{

								/// compose bulk messages for sending.
								//composerResult = await Compose(emailTemplate, module, emailField, subject, query, ids, language, emailId, cloudantClient, emailClient);
								composerResult = await Compose(emailQueueItem, emailTemplate, module, emailField, subject, query, ids, isAllSelected, emailNotification.CreatedById, language, emailId, databaseContext, platformUserRepository, tenantRepository, _configuration, emailClient, emailNotification.AttachmentLink, emailNotification.AttachmentName, Cc, Bcc, appUser);

								/// send composed messages through selected provider.
								emailResponse = await emailClient.Send(composerResult.Messages);

								/// set main status to the response status.
								bulkEMailStatus = emailResponse.Status;
							}

							/// set status and update short message record.
							///
							emailNotification.Status = bulkEMailStatus;

							databaseContext.Entry(emailNotification).State = EntityState.Modified;
							composerResult.ProviderResponse = emailResponse.Status.ToString();

							emailNotification.Result = JsonConvert.SerializeObject(composerResult);
							databaseContext.SaveChanges();

							///Cleanup
							composerResult.DetailedMessageStatusList?.Clear();
							composerResult.Messages?.Clear();

						}

					}
				}
			}
			catch (Exception ex)
			{
				ErrorHandler.LogError(ex, $"EMail Client has failed while sending a short message template with id:{emailId} of tenant: {emailQueueItem.TenantId}.");
				bulkEMailStatus = NotificationStatus.SystemError;
			}
			Email.Messaging.SendEMailStatusNotification(emailOwner, emailTemplate, senderAlias, senderEMail, moduleName, queueDate, bulkEMailStatus, composerResult.Successful, composerResult.NotAllowed, composerResult.NoAddress, emailQueueItem.TenantId, _configuration, _serviceScopeFactory, appUser);

			/// always return true to say queue that the job has done.
			return true;
		}

		/// <summary>
		/// Composes email messages with a given id array or query.
		/// </summary>
		/// <param name="messageBody"></param>
		/// <param name="module"></param>
		/// <param name="query"></param>
		/// <param name="ids"></param>
		/// <param name="language"></param>
		/// <param name="cloudantClient"></param>
		/// <returns></returns>
		public async Task<EMailComposerResult> Compose(MessageDTO messageDto, string messageBody, Module module, string emailField, string subject, string query, string[] ids, bool isAllSelected, int userId, string language, string emailId, TenantDBContext dbContext, PlatformUserRepository platformUserRepository, TenantRepository tenantRepository, IConfiguration configuration, EMailProvider emailClient, string attachmentLink, string attachmentName, string Cc, string Bcc, UserItem appUser)
		{
			/// create required parameters for composing.
			Regex templatePattern = new Regex(@"{(.*?)}");
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

			IList<Message> messages = new List<Message>();
			IList<string> messageFields = new List<string>();
			JArray messageStatusList = new JArray();
			int successful = 0,
				noAddress = 0,
				notAllowed = 0;

			var subscriber = await platformUserRepository.GetSettings(userId);
			var tenant = await tenantRepository.GetWithSettingsAsync(messageDto.TenantId);

			string culture = subscriber.Setting.Culture;
			string lang = subscriber.Setting.Language;

			if (!tenant.App.UseTenantSettings)
			{
				culture = tenant.App.Setting.Culture;
				lang = tenant.App.Setting.Language;
			}
			else if (!tenant.UseUserSettings)
			{
				culture = tenant.Setting.Culture;
				lang = tenant.Setting.Language;
			}
			else
			{
				culture = subscriber.Setting.Culture;
				lang = subscriber.Setting.Language;
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
                if (isAllSelected)
				{
					//Query with filtered or non filtered selectedAll ids..
					var serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();
					var findRequest = JsonConvert.DeserializeObject<FindRequest>(query, serializerSettings);

					//Set find request limit to our maximum value 3000 unlike filter default
					findRequest.Limit = 30000;

					using (var _scope = _serviceScopeFactory.CreateScope())
					{
						var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

						using (var recordRepository = new RecordRepository(databaseContext, _configuration))
						{
							recordRepository.UserId = userId;

							var records = recordRepository.Find(module.Name, findRequest, false);

							if (records.Count > 0)
							{
								var idListSelectAll = new List<string>();

								foreach (JObject record in records)
								{
									idListSelectAll.Add(record["id"].ToString());
								}

								ids = idListSelectAll.ToArray();
							}
						}
					}
				}

				if (ids?.Length > 0)
				{
					using (var _scope = _serviceScopeFactory.CreateScope())
					{
						var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
						using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
						using (var picklistRepository = new PicklistRepository(databaseContext, _configuration))
						using (var recordRepository = new RecordRepository(databaseContext, _configuration))
						{

							moduleRepository.CurrentUser = picklistRepository.CurrentUser = recordRepository.CurrentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

                            foreach (string recordId in ids)
							{
								var status = MessageStatusEnum.Successful;
								var lookupModules = await RecordHelper.GetLookupModules(module, moduleRepository, tenantLanguage: tenant.Setting.Language);
								var record = recordRepository.GetById(module, int.Parse(recordId), false, lookupModules);

								if (!record[emailField].IsNullOrEmpty())
								{
									if (!Utils.IsValidEmail(record[emailField].ToString()))
									{
										status = MessageStatusEnum.InvalidField;
										noAddress++;
									}
								}
								else
								{
									status = MessageStatusEnum.MissingField;
									noAddress++;
								}

								if (!record["email_opt_out"].IsNullOrEmpty() && status == MessageStatusEnum.Successful)
								{
									var isAllowedEmail = (bool)record["email_opt_out"];

									if (isAllowedEmail)
									{
										status = MessageStatusEnum.OptedOut;
										notAllowed++;
									}
								}

								record = await RecordHelper.FormatRecordValues(module, record, moduleRepository, picklistRepository, _configuration, tenant.GuidId, language, culture, 180, lookupModules, true);
								string formattedMessage = FormatMessage(messageFields, messageBody, record);

								JObject messageStatus = new JObject();
								messageStatus["email"] = record[emailField]?.ToString();
								messageStatus["message"] = formattedMessage;
								messageStatus["status"] = status.ToString();
								messageStatus["email_id"] = emailId;
								messageStatus["record_primary_value"] = record[emailField]?.ToString();
								messageStatus["module_id"] = module.Id;
								messageStatus["type"] = "email_detail";
								messageStatus["record_id"] = record["id"].ToString();

								/// add status object to the status list.
								messageStatusList.Add(messageStatus);

								if (status == MessageStatusEnum.Successful)
								{
									/// create a message object and add it to the list.
									Message emailMessage = new Message();
									emailMessage.Recipients.Add(record[emailField].ToString());
									emailMessage.Body = formattedMessage;
									emailMessage.Subject = subject;
									emailMessage.AttachmentLink = attachmentLink;
									emailMessage.AttachmentName = attachmentName;
									emailMessage.Cc = Cc;
									emailMessage.Bcc = Bcc;
									messages.Add(emailMessage);
									successful++;
								}
							}
						}



					}
				}
			}

			return new EMailComposerResult()
			{
				NotAllowed = notAllowed,
				Successful = successful,
				NoAddress = noAddress,
				Messages = messages,
				DetailedMessageStatusList = messageStatusList
			};
		}
	}
}
