using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Jobs.Messaging.EMail;
using PrimeApps.App.Jobs.Messaging.SMS;
using PrimeApps.Model.Common.Messaging;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Controllers
{
	[Route("api/messaging")]
	[Authorize]
	public class MessagingController : ApiBaseController
	{
		private IMessagingRepository _messagingRepository;
		private ISettingRepository _settingRepository;
		private IConfiguration _configuration;
		private IServiceScopeFactory _serviceScopeFactory;
		private ITemplateRepository _templateRepository;

		public MessagingController(IMessagingRepository messagingRepository, ISettingRepository settingRepository, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, ITemplateRepository templateRepository)
		{
			_messagingRepository = messagingRepository;
			_settingRepository = settingRepository;
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
			_templateRepository = templateRepository;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_messagingRepository, PreviewMode, TenantId, AppId);
			SetCurrentUser(_settingRepository, PreviewMode, TenantId, AppId);
			SetCurrentUser(_templateRepository, PreviewMode, TenantId, AppId);

			base.OnActionExecuting(context);
		}

		/// <summary>
		/// Sends bulk short message
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[Route("send_sms")]
		public async Task<IActionResult> SendSMS([FromBody]SMSRequest request)
		{
			var randomRevNumber = Helpers.Utils.CreateRandomString(20);

			var ids = request.IsAllSelected ? "ALL" : string.Join(",", request.Ids);
			var smsNotification = new Notification()
			{
				NotificationType = Model.Enums.NotificationType.Sms,
				ModuleId = request.ModuleId,
				Ids = ids,
				Lang = AppUser.TenantLanguage,
				Status = Model.Enums.NotificationStatus.Queued,
				Template = request.TemplateId > 0 ? request.TemplateId.ToString() : request.Message,
				QueueDate = DateTime.UtcNow,
				PhoneField = request.PhoneField,
				Rev = randomRevNumber,
				Query = request.Query,
			};
			var smsMessage = await _messagingRepository.Create(smsNotification);
			if (smsMessage != null)
			{
				/// create sms queue object
				MessageDTO queuedMessage = new MessageDTO();
				queuedMessage.Id = smsMessage.Id.ToString();
				queuedMessage.Rev = randomRevNumber;
				queuedMessage.TenantId = AppUser.TenantId;
				queuedMessage.Type = MessageTypeEnum.SMS;
				queuedMessage.AccessLevel = AccessLevelEnum.System;
				try
				{
					/// send message to the queue.
					//await ServiceBus.SendMessage("sms", queuedMessage, DateTime.UtcNow);
					Hangfire.BackgroundJob.Enqueue<SMSClient>(sms => sms.Process(queuedMessage, AppUser));
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
			else
			{
				return StatusCode(500);
				//return InternalServerError();
			}

			return Ok();
		}

		/// <summary>
		/// Sends bulk emails.
		/// </summary>
		/// <param name="request"></param>
		/// <returns></returns>
		[Route("send_email_job")]
		public async Task<IActionResult> SendEMailJob([FromBody]EmailJobRequest request)
		{
			var randomRevNumber = Utils.CreateRandomString(20);

			var ids = request.IsAllSelected ? "ALL" : string.Join(",", request.Ids);
			var emailNotification = new Notification()
			{
				NotificationType = NotificationType.Email,
				ModuleId = request.ModuleId,
				Rev = randomRevNumber,
				Query = request.Query,
				AttachmentContainer = request.AttachmentContainer,
				EmailField = request.EMailField,
				Ids = ids,
				Lang = AppUser.TenantLanguage,
				Status = NotificationStatus.Queued,
				Template = request.TemplateId.ToString(),
				Subject = request.Subject,
				SenderAlias = request.SenderAlias,
				SenderEmail = request.SenderEMail,
				QueueDate = DateTime.UtcNow,
				AttachmentLink = request.AttachmentLink,
				AttachmentName = request.AttachmentName,
				Cc = request.Cc,
				Bcc = request.Bcc
			};

			var emailMessage = await _messagingRepository.Create(emailNotification);
			if (emailMessage != null)
			{
				MessageDTO queuedMessage = new MessageDTO();
				queuedMessage.Id = emailMessage.Id.ToString();
				queuedMessage.Rev = randomRevNumber;
				queuedMessage.TenantId = AppUser.TenantId;
				queuedMessage.Type = MessageTypeEnum.EMail;
				queuedMessage.AccessLevel = request.ProviderType;
				try
				{
					/// send message to the queue.(Same queue with sms, does not affect something on service bus)
					//await ServiceBus.SendMessage("sms", queuedMessage, DateTime.UtcNow);
					Hangfire.BackgroundJob.Enqueue<EMailClient>(email => email.Process(queuedMessage, AppUser));
				}
				catch (Exception ex)
				{
					throw ex;
				}
			}
			else
			{
				return StatusCode(500);
				//return InternalServerError();
			}

			return Ok();
		}

		/// <summary>
		/// Sends email from an external source like an api etc.
		/// </summary>
		/// <returns></returns>
		[Route("send_external_email")]
		public async Task<IActionResult> SendExternalEmail([FromBody]ExternalEmail emailRequest)
		{
			if (emailRequest.TemplateWithBody != null && emailRequest.ToAddresses.Length > 0)
			{
				if (!emailRequest.TemplateWithBody.Contains("html"))
				{
					var language = !string.IsNullOrEmpty(emailRequest.Language) ? emailRequest.Language.ToEnum<LanguageType>() : AppUser.Language.ToEnum<LanguageType>();
					var template = _templateRepository.GetByCode(emailRequest.TemplateWithBody, language);
					emailRequest.TemplateWithBody = template.Content;
					emailRequest.Subject = template.Subject;
				}

				if (emailRequest.Cc == null)
					emailRequest.Cc = "";

				if (emailRequest.Bcc == null)
					emailRequest.Bcc = "";

				var externalEmail = new Email(emailRequest.Subject, emailRequest.TemplateWithBody, _configuration, _serviceScopeFactory);

				foreach (var emailRecipient in emailRequest.ToAddresses)
				{
					externalEmail.AddRecipient(emailRecipient);
				}

				var moduleId = emailRequest.ModuleId ?? 0;
				var recordId = emailRequest.RecordId ?? 0;

				await externalEmail.AddToQueue(AppUser.TenantId, moduleId, recordId, cc: emailRequest.Cc, bcc: emailRequest.Bcc, from: emailRequest.FromEmail, fromName: emailRequest.FromName, appUser: AppUser, addRecordSummary: false);

				return Ok("This method has been deprecated! \n" + emailRequest.ToAddresses.Count());
			}

			return BadRequest();
		}

		[Route("send_email")]
		public async Task<IActionResult> SendEmail([FromBody]EmailRequest emailRequest)
		{
			if (emailRequest.ToAddresses.Length > 0)
			{
				if (emailRequest.Cc == null)
					emailRequest.Cc = "";

				if (emailRequest.Bcc == null)
					emailRequest.Bcc = "";

				var externalEmail = new Email(emailRequest.Template.Subject, emailRequest.Template.Content, _configuration, _serviceScopeFactory);

				foreach (var emailRecipient in emailRequest.ToAddresses)
				{
					externalEmail.AddRecipient(emailRecipient);
				}

				var moduleId = emailRequest.ModuleId ?? 0;
				var recordId = emailRequest.RecordId ?? 0;

				await externalEmail.AddToQueue(AppUser.TenantId, moduleId, recordId, cc: emailRequest.Cc, bcc: emailRequest.Bcc, from: emailRequest.FromEmail, fromName: emailRequest.FromName, appUser: AppUser, addRecordSummary: false);

				return Ok(emailRequest.ToAddresses.Count());
			}

			return BadRequest();
		}

		/// <summary>
		/// Updates SMS server settings.
		/// </summary>
		/// <param name="newSettings"></param>
		/// <returns></returns>
		[Route("update_sms_settings")]
		public async Task<IActionResult> UpdateSMSSettings([FromBody]JObject newSettings)
		{
			IList<Setting> settings = new List<Setting>();
			foreach (var data in newSettings)
			{
				Setting setting = new Setting()
				{
					Key = data.Key,
					Value = data.Value?.ToString(),
					Type = Model.Enums.SettingType.SMS,
				};
				settings.Add(setting);
			}

			if (settings.Count > 0)
			{
				await RemoveSMSSettings();
			}

			var count = await _settingRepository.AddSettings(settings);
			if (count > 0)
			{
				return Ok(count);
			}

			return BadRequest();
		}

		/// <summary>
		/// Updates email server settings.
		/// </summary>
		/// <param name="newSettings"></param>
		/// <returns></returns>
		[Route("update_email_settings")]
		public async Task<IActionResult> UpdateEMailSettings([FromBody]JObject newSettings)
		{
			IList<Setting> settings = new List<Setting>();
			foreach (var data in newSettings)
			{
				Setting setting = new Setting()
				{
					Key = data.Key,
					Value = data.Value?.ToString(),
					Type = Model.Enums.SettingType.Email,
				};

				if (data.Key == "password")
					setting.Value = CryptoHelper.Encrypt(data.Value?.ToString());

				settings.Add(setting);
			}

			if (settings.Count > 0)
			{
				await RemoveEMailSettings();
			}

			var count = await _settingRepository.AddSettings(settings);
			if (count > 0)
			{
				return Ok(count);
			}

			return BadRequest();
		}

		/// <summary>
		/// Update personal email server settings.
		/// </summary>
		/// <param name="newSettings"></param>
		/// <returns></returns>
		[Route("update_personal_email_settings")]
		public async Task<IActionResult> UpdatePersonalEMailSettings([FromBody]JObject newSettings)
		{
			IList<Setting> settings = new List<Setting>();
			foreach (var data in newSettings)
			{
				/// we set here the local (tenant user id) to make this settings personal.
				Setting setting = new Setting()
				{
					Key = data.Key,
					Value = data.Value?.ToString(),
					Type = SettingType.Email,
					UserId = AppUser.Id
				};

				if (data.Key == "host" && data.Value.ToString().Contains("yandex"))
				{
					setting.Value = "smtp.yandex.ru";
				}

				if (data.Key == "password")
					setting.Value = CryptoHelper.Encrypt(data.Value?.ToString());

				settings.Add(setting);
			}

			if (settings.Count > 0)
			{
				await RemovePersonalEMailSettings();
			}

			var count = await _settingRepository.AddSettings(settings);
			if (count > 0)
			{
				return Ok(count);
			}

			return BadRequest();
		}

		/// <summary>
		/// Removes email settings
		/// </summary>
		/// <returns></returns>
		[Route("remove_email_settings")]
		public async Task<IActionResult> RemoveEMailSettings()
		{
			var result = await _settingRepository.DeleteAsync(Model.Enums.SettingType.Email);
			if (result)
			{
				return Ok();
			}

			return NotFound();
		}

		/// <summary>
		/// Removes email settings
		/// </summary>
		/// <returns></returns>
		[Route("remove_personal_email_settings")]
		public async Task<IActionResult> RemovePersonalEMailSettings()
		{
			var result = await _settingRepository.DeleteAsync(Model.Enums.SettingType.Email, AppUser.Id);
			if (result)
			{
				return Ok();
			}

			return NotFound();
		}

		/// <summary>
		/// Removes sms settings.
		/// </summary>
		/// <returns></returns>
		[Route("remove_sms_settings")]
		public async Task<IActionResult> RemoveSMSSettings()
		{
			var result = await _settingRepository.DeleteAsync(Model.Enums.SettingType.SMS);
			if (result)
			{
				return Ok();
			}

			return NotFound();
		}

		/// <summary>
		/// Gets configuration for all messaging services.
		/// </summary>
		/// <returns></returns>
		[Route("get_config")]
		public async Task<JObject> GetConfig()
		{
			var config = new JObject();
			var settings = await _settingRepository.GetAllSettings(AppUser.Id);

			if (settings != null && settings.Count > 0)
			{
				if (settings.Any(r => r.Type == Model.Enums.SettingType.SMS))
				{
					config["SMS"] = new JObject();

					var smsSettings = settings.Where(r => r.Type == Model.Enums.SettingType.SMS);

					foreach (var smsSetting in smsSettings)
					{
						if (smsSetting.Key != "password")
							config["SMS"][smsSetting.Key] = smsSetting.Value;
					}
				}

				if (settings.Any(r => r.Type == Model.Enums.SettingType.Email && r.UserId == null))
				{
					config["SystemEMail"] = new JObject();

					var emailSettings = settings.Where(r => r.Type == Model.Enums.SettingType.Email && r.UserId == null);

					foreach (var emailSetting in emailSettings)
					{
						if (emailSetting.Key != "password")
						{
							if (emailSetting.Key != "senders")
							{
								config["SystemEMail"][emailSetting.Key] = emailSetting.Value;
							}
							else
							{
								config["SystemEMail"][emailSetting.Key] = JArray.Parse(emailSetting.Value);
							}
						}
					}
				}

				if (settings.Any(r => r.Type == Model.Enums.SettingType.Email && r.UserId == AppUser.Id))
				{
					config["PersonalEMail"] = new JObject();

					var emailSettings = settings.Where(r => r.Type == Model.Enums.SettingType.Email && r.UserId == AppUser.Id);

					foreach (var emailSetting in emailSettings)
					{
						if (emailSetting.Key != "password")
						{
							if (emailSetting.Key != "senders")
							{
								config["PersonalEMail"][emailSetting.Key] = emailSetting.Value;
							}
							else
							{
								config["PersonalEMail"][emailSetting.Key] = JArray.Parse(emailSetting.Value);
							}
						}
					}
				}
			}

			return config;
		}

	}
}