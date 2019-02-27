using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PrimeApps.Studio.Helpers
{
	public class Email
	{
		private IConfiguration _configuration;
		private IServiceScopeFactory _serviceScopeFactory;

		public Email(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
		{
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}

		public object Messaging { get; internal set; }

		public bool TransmitMail(MailMessage mail)
		// public bool TransmitMail(EmailEntry mail)
		{
			bool status = false;

			// get a record by the queue algorithm from database.

			if (mail != null)
			{
				// create smtp client and mail message objects
				SmtpClient smtpClient;
				var smtpHost = "EmailSMTPHost";
				var smtpPort = "EmailSMTPPort";
				var smtpUser = "EmailSMTPUser";
				var smtpPassword = "EmailSMTPPassword";
				var emailSmtpEnableSsl = _configuration.GetValue("AppSettings:EmailSMTPEnableSsl", string.Empty);		
				var smtpHost_ = _configuration.GetValue("AppSettings:" + smtpHost + '"', string.Empty);
				var smtpPortSetting = _configuration.GetValue("AppSettings:" + smtpPort + '"', string.Empty);
				// get configuration settings from appsetting and apply them.
				if (!string.IsNullOrEmpty(smtpHost_) && !string.IsNullOrEmpty(smtpPortSetting) && !string.IsNullOrEmpty(emailSmtpEnableSsl))
				{
					var smtpUserSetting = _configuration.GetValue("AppSettings:" + smtpUser + '"', string.Empty);
					var smtpPasswordSetting = _configuration.GetValue("AppSettings:" + smtpPassword + '"', string.Empty);

					if (!string.IsNullOrEmpty(smtpUserSetting) && !string.IsNullOrEmpty(smtpPasswordSetting))
					{
						smtpClient = new SmtpClient(smtpHost_, int.Parse(smtpPortSetting))
						{
							UseDefaultCredentials = false,
							// set credentials
							Credentials = new NetworkCredential(smtpUserSetting, smtpPasswordSetting),
							DeliveryFormat = SmtpDeliveryFormat.International,
							DeliveryMethod = SmtpDeliveryMethod.Network,
							EnableSsl = bool.Parse(emailSmtpEnableSsl)
						};

						// transmit it.
						smtpClient.Send(mail);

						// set status to true
						status = true;
					}			
				}
			}

			// return status.
			return status;
		}

	}
}
