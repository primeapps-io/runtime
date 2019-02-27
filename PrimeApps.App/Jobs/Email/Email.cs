using PrimeApps.App.Helpers;
using PrimeApps.Model.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using RecordHelper = PrimeApps.Model.Helpers.RecordHelper;
using PrimeApps.App.Jobs.Messaging;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;

namespace PrimeApps.App.Jobs.Email
{
	/// <summary>
	/// Schedules and send email messages application wide.
	/// </summary>
	public class Email
	{
		private static IConfiguration _configuration;
		private static IServiceScopeFactory _serviceScopeFactory;

		public Email(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
		{
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}

		public static object Messaging { get; internal set; }

		/// <summary>
		/// Checks database for new email records and transmits it if there is new one.
		/// </summary>
		/// <returns></returns>
		[QueueCustom]
		public bool TransmitMail(EmailEntry mail)
		// public bool TransmitMail(EmailEntry mail)
		{
			bool status = false;

			// get a record by the queue algorithm from database.

			if (mail != null)
			{
				if (mail.EmailTo == null || mail.EmailTo.Count < 1)
					throw new Exception("EmailTo cannot be null.");

				// create smtp client and mail message objects
				SmtpClient smtpClient = null;
				MailMessage myMessage;
				var smtpHost = "EmailSMTPHost";
				var smtpPort = "EmailSMTPPort";
				var smtpUser = "EmailSMTPUser";
				var smtpPassword = "EmailSMTPPassword";
				var emailSMTPEnableSsl = _configuration.GetValue("AppSettings:EmailSMTPEnableSsl", string.Empty);
				var smtpHost_ = _configuration.GetValue("AppSettings:" + smtpHost + '"', string.Empty);
				var smtpPort_ = _configuration.GetValue("AppSettings:" + smtpPort + '"', string.Empty);
				if (!string.IsNullOrEmpty(smtpHost_) && !string.IsNullOrEmpty(smtpPort_))
				{
					var smtpUser_ = _configuration.GetValue("AppSettings:" + smtpUser + '"', string.Empty);
					var smtpPassword_ = _configuration.GetValue("AppSettings:" + smtpPassword + '"', string.Empty);

					if (!string.IsNullOrEmpty(smtpUser_) && !string.IsNullOrEmpty(smtpPassword_) && !string.IsNullOrEmpty(emailSMTPEnableSsl))
					{
						smtpClient = new SmtpClient(smtpHost_, int.Parse(smtpPort_))
						{
							UseDefaultCredentials = false,
							// set credentials
							Credentials = new NetworkCredential(smtpUser_, smtpPassword_),
							DeliveryFormat = SmtpDeliveryFormat.International,
							DeliveryMethod = SmtpDeliveryMethod.Network,
							EnableSsl = bool.Parse(emailSMTPEnableSsl)
						};
					}
				}
				// get configuration settings from appsetting and apply them.

				var emailAddress = new EmailAddressAttribute();

				// generate email message
				myMessage = new MailMessage()
				{
					From = new MailAddress(mail.EmailFrom, mail.FromName),
					Subject = mail.Subject,
					Body = mail.Body,
					IsBodyHtml = true
				};

				foreach (var to in mail.EmailTo)
				{
					if (string.IsNullOrWhiteSpace(to) || !emailAddress.IsValid(to))
						continue;

					//kullanıcı e-postası pre__ ile başlayan kullanıcıları filtreleme
					myMessage.To.Add(Regex.Replace(to, "^pre__", ""));
				}

				if (!string.IsNullOrWhiteSpace(mail.CC))
				{
					var ccList = mail.CC.Split(',');
					foreach (var cc in ccList)
					{
						if (string.IsNullOrWhiteSpace(cc) || !emailAddress.IsValid(cc))
							continue;

						myMessage.CC.Add(cc);
					}
				}

				if (!string.IsNullOrWhiteSpace(mail.Bcc))
				{
					var bccList = mail.Bcc.Split(',');

					foreach (var bcc in bccList)
					{
						if (string.IsNullOrWhiteSpace(bcc) || !emailAddress.IsValid(bcc))
							continue;

						myMessage.Bcc.Add(bcc);
					}
				}

				// transmit it.
				smtpClient.Send(myMessage);

				// set status to true
				status = true;
			}

			// return status.
			return status;
		}

		[QueueCustom]
		public bool TransmitMail(EmailEntry email, int tenantId, int moduleId, int recordId, UserItem appUser, bool addRecordSummary = true)
		{
			bool status = false;

			if (email != null)
			{
				// create smtp client and mail message objects
				SmtpClient smtpClient = null;
				MailMessage myMessage;
				var smtpHost = "EmailSMTPHost";
				var smtpPort = "EmailSMTPPort";
				var smtpUser = "EmailSMTPUser";
				var smtpPassword = "EmailSMTPPassword";
				var emailSMTPEnableSsl = _configuration.GetValue("AppSettings:EmailSMTPEnableSsl", string.Empty);
				var smtpHost_ = _configuration.GetValue("AppSettings:" + smtpHost + '"', string.Empty);
				var smtpPort_ = _configuration.GetValue("AppSettings:" + smtpPort + '"', string.Empty);
				if (!string.IsNullOrEmpty(smtpHost_) && !string.IsNullOrEmpty(smtpPort_))
				{
					var smtpUser_ = _configuration.GetValue("AppSettings:" + smtpUser + '"', string.Empty);
					var smtpPassword_ = _configuration.GetValue("AppSettings:" + smtpPassword + '"', string.Empty);

					if (!string.IsNullOrEmpty(smtpUser_) && !string.IsNullOrEmpty(smtpPassword_) && !string.IsNullOrEmpty(emailSMTPEnableSsl))
					{

						// get configuration settings from appsetting and apply them.
						smtpClient = new SmtpClient(smtpHost_, int.Parse(smtpPort_))
						{
							UseDefaultCredentials = false,
							// set credentials
							Credentials = new NetworkCredential(smtpUser_, smtpPassword_),
							DeliveryFormat = SmtpDeliveryFormat.International,
							DeliveryMethod = SmtpDeliveryMethod.Network,
							EnableSsl = bool.Parse(emailSMTPEnableSsl)
						};
					}
				}
				// parse subject and body
				var subject = AsyncHelpers.RunSync(() => ParseDynamicContent(email.Subject, tenantId, moduleId, recordId, appUser, false));
				var body = AsyncHelpers.RunSync(() => ParseDynamicContent(email.Body, tenantId, moduleId, recordId, appUser, addRecordSummary));

				var emailAddress = new EmailAddressAttribute();

				// generate email message
				myMessage = new MailMessage()
				{
					From = new MailAddress(email.EmailFrom, email.FromName),
					Subject = subject,
					Body = body,
					IsBodyHtml = true
				};

				foreach (var to in email.EmailTo)
				{
					if (string.IsNullOrWhiteSpace(to) || !emailAddress.IsValid(to))
						continue;

					myMessage.To.Add(to);
				}

				if (myMessage.To.Count < 1)
					return true;


				if (!string.IsNullOrWhiteSpace(email.CC))
				{
					var ccList = email.CC.Split(',');
					foreach (var cc in ccList)
					{
						myMessage.CC.Add(cc);
					}
				}

				if (!string.IsNullOrWhiteSpace(email.Bcc))
				{
					var bccList = email.Bcc.Split(',');

					foreach (var bcc in bccList)
					{
						myMessage.Bcc.Add(bcc);
					}
				}

				// transmit it.
				smtpClient.Send(myMessage);

				// set status to true
				status = true;
			}

			// return status.
			return status;
		}

		private static async Task<string> ParseDynamicContent(string content, int tenantId, int moduleId, int recordId, UserItem appUser, bool addRecordSummary = true)
		{
			var pattern = new Regex(@"{(.*?)}");
			var contentFields = new List<string>();
			var matches = pattern.Matches(content);

			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";


			Tenant subscriber = null;
			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var platformDatabaseContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var tenantDatabaseContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
				var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();

				using (var tenantRepository = new TenantRepository(platformDatabaseContext, _configuration, cacheHelper))
				{
					subscriber = await tenantRepository.GetAsync(tenantId);
				}

				/*
			PlatformUser subscriber = null;

			using (var platformDBContext = new PlatformDBContext())
			using (var platformUserRepository = new PlatformUserRepository(platformDBContext))
			{
				subscriber = await platformUserRepository.GetWithTenant(tenantId);
			}
			*/
				foreach (object match in matches)
				{
					string fieldName = match.ToString().Replace("{", "").Replace("}", "");

					if (!contentFields.Contains(fieldName))
					{
						contentFields.Add(fieldName);
					}
				}

				using (var moduleRepository = new ModuleRepository(tenantDatabaseContext, _configuration))
				using (var picklistRepository = new PicklistRepository(tenantDatabaseContext, _configuration))
				using (var recordRepository = new RecordRepository(tenantDatabaseContext, _configuration))
				{

					moduleRepository.CurrentUser = picklistRepository.CurrentUser = recordRepository.CurrentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

					var module = await moduleRepository.GetById(moduleId);
					var lookupModules = await RecordHelper.GetLookupModules(module, moduleRepository, tenantLanguage: subscriber.Setting.Language);
					var record = recordRepository.GetById(module, recordId, false, lookupModules, true);

					if (!record.IsNullOrEmpty())
					{
						record = await RecordHelper.FormatRecordValues(module, record, moduleRepository, picklistRepository, _configuration, subscriber.GuidId, subscriber.Setting.Language, subscriber.Setting.Culture, 180, lookupModules, true);

						if (contentFields.Count > 0)
						{
							foreach (var field in contentFields)
							{
								if (!record[field].IsNullOrEmpty())
									content = content.Replace($"{{{field}}}", record[field].ToString());
								else
									content = content.Replace($"{{{field}}}", "");
							}
						}

						if (addRecordSummary)
						{
							var recordTable = "";
							var recordRow = "<table style=\"width:100%;height:30px\"; border=\"0\" cellpadding=\"0\" cellspacing=\"0\" ><tr><td style=\"border: solid 1px #e5e8ec; background-color:#f1f1f1;width:25%;padding-left: 10px;\">{label}</td><td style=\"border:solid 1px #e5e8ec;width:40%;padding-left: 10px;\">{value}</td></tr></table>" + "\n";

							var fields = module.Fields.Where(x => x.DisplayDetail && x.Deleted != true && x.Validation != null && x.Validation.Required.HasValue && x.Validation.Required.Value && (x.Permissions == null || x.Permissions.Count < 1)).OrderBy(x => x.Order);
							if (module.Name == "izinler" && !record["calisan.id"].IsNullOrEmpty())
							{
								var format = "Gün";
								if (!record["izin_turu.saatlik_kullanim_yapilir"].IsNullOrEmpty() && record["izin_turu.saatlik_kullanim_yapilir"].HasValues)
									format = "Saat";

								var tarih = (string)record["baslangic_tarihi"] + " - " + record["bitis_tarihi"] + " / " + record["hesaplanan_alinacak_toplam_izin"] + " " + format;

								if (record["calisan.name_surname"].IsNullOrEmpty())
								{
									recordTable += recordRow.Replace("{label}", "Adı Soyadı").Replace("{value}", !record["calisan.ad_soyad"].IsNullOrEmpty() ? record["calisan.ad_soyad"].ToString() : "");
									recordTable += recordRow.Replace("{label}", "Unvanı").Replace("{value}", !record["calisan.unvan"].IsNullOrEmpty() ? record["calisan.unvan"].ToString() : "");
									recordTable += recordRow.Replace("{label}", "Departman").Replace("{value}", !record["calisan.departman"].IsNullOrEmpty() ? record["calisan.departman"].ToString() : "");
									recordTable += recordRow.Replace("{label}", "İzin Türü").Replace("{value}", !record["izin_turu.adi"].IsNullOrEmpty() ? record["izin_turu.adi"].ToString() : "");
									recordTable += recordRow.Replace("{label}", "Tarih").Replace("{value}", tarih);
									recordTable += recordRow.Replace("{label}", "Açıklama").Replace("{value}", !record["aciklama"].IsNullOrEmpty() ? record["aciklama"].ToString() : "");
								}
								else
								{
									recordTable += recordRow.Replace("{label}", "Name Surname").Replace("{value}", !record["calisan.name_surname"].IsNullOrEmpty() ? record["calisan.name_surname"].ToString() : "");
									recordTable += recordRow.Replace("{label}", "Position").Replace("{value}", !record["calisan.position"].IsNullOrEmpty() ? record["calisan.position"].ToString() : "");
									recordTable += recordRow.Replace("{label}", "Departmant").Replace("{value}", !record["calisan.department"].IsNullOrEmpty() ? record["calisan.department"].ToString() : "");
									recordTable += recordRow.Replace("{label}", "Leave Type").Replace("{value}", !record["izin_turu.adi"].IsNullOrEmpty() ? record["izin_turu.adi"].ToString() : "");
									recordTable += recordRow.Replace("{label}", "Date").Replace("{value}", tarih);
									recordTable += recordRow.Replace("{label}", "Description").Replace("{value}", !record["aciklama"].IsNullOrEmpty() ? record["aciklama"].ToString() : "");
								}

							}
							else
							{
								foreach (var field in fields)
								{
									if (!record[field.Name].IsNullOrEmpty() && field.Name != "created_by" && field.Name != "updated_by" && field.Name != "created_at" && field.Name != "updated_at")
										recordTable += recordRow.Replace("{label}", field.LabelTr).Replace("{value}", record[field.Name].ToString());

								}

							}
							if (!record["process_status_order"].IsNullOrEmpty() && (int)record["process_status_order"] != 1 && (int)record["process_status_order"] != 0)
							{
								var userDataObj = new JObject();
								var findRequest = new FindRequest();
								switch ((int)record["process_status_order"])
								{
									case 2:
										findRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = record["custom_approver"].ToString(), No = 1 } }, Limit = 1 };
										break;
									case 3:
										findRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = record["custom_approver_2"].ToString(), No = 1 } }, Limit = 1 };
										break;
									case 4:
										findRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = record["custom_approver_3"].ToString(), No = 1 } }, Limit = 1 };
										break;
									case 5:
										findRequest = new FindRequest { Filters = new List<Filter> { new Filter { Field = "email", Operator = Operator.Is, Value = record["custom_approver_4"].ToString(), No = 1 } }, Limit = 1 };
										break;
								}

								var userData = recordRepository.Find("users", findRequest, false);
								userDataObj = (JObject)userData.First();

								if (subscriber.Setting.Culture.Contains("tr"))
									recordTable += recordRow.Replace("{label}", "Önceki Onaylayan").Replace("{value}", !userDataObj["full_name"].IsNullOrEmpty() ? userDataObj["full_name"].ToString() : "");
								else
									recordTable += recordRow.Replace("{label}", "Previous Approver").Replace("{value}", !userDataObj["full_name"].IsNullOrEmpty() ? userDataObj["full_name"].ToString() : "");
							}

							content = content.Replace("[[recordTable]]", recordTable);
						}
						else
						{
							content = content.Replace("[[recordTable]]", "");
						}
					}
				}
			}

			return content;
		}
	}
}