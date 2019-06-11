using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Studio.ActionFilters;
using PrimeApps.Studio.Jobs.Messaging;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using RecordHelper = PrimeApps.Model.Helpers.RecordHelper;

namespace PrimeApps.Studio.Jobs.Email
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
				var smtpHostSetting = _configuration.GetValue("AppSettings:EmailSMTPHost", string.Empty);

				if (!string.IsNullOrEmpty(smtpHostSetting))
				{
					var smtpUserSetting = _configuration.GetValue("AppSettings:EmailSMTPUser", string.Empty);
					var smtpPasswordSetting = _configuration.GetValue("AppSettings:EmailSMTPPassword", string.Empty);
                    var emailSMTPEnableSsl = _configuration.GetValue("AppSettings:EmailSMTPEnableSsl", string.Empty);		
                    var smtpPortSetting = _configuration.GetValue("AppSettings:EmailSMTPPort", string.Empty);

                    smtpClient = new SmtpClient(smtpHostSetting, int.Parse(smtpPortSetting))
                    {
                        UseDefaultCredentials = false,
                        // set credentials
                        Credentials = new NetworkCredential(smtpUserSetting, smtpPasswordSetting),
                        DeliveryFormat = SmtpDeliveryFormat.International,
                        DeliveryMethod = SmtpDeliveryMethod.Network,
                        EnableSsl = bool.Parse(emailSMTPEnableSsl)
                    };
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
				if (myMessage.To.Count > 1)
				smtpClient.Send(myMessage);

				// set status to true
				status = true;
			}

			// return status.
			return status;
		}
	}
}