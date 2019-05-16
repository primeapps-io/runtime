using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Studio.Jobs.Messaging;

namespace PrimeApps.Studio.Helpers
{
    public class Email
    {
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        /// <summary>
		/// The pattern of template placeholders.
		/// </summary>
		private string templatePattern = @"({)(\w*)(})";
        private string dataPattern = @"({)(:)(\w*)(})";

        /// <summary>
        /// List of the recipients.
        /// </summary>
        private IList<string> toList = new List<string>();
        private string footer;

        /// <summary>
        /// The regex helper for placeholders.
        /// </summary>
        private Regex dataRegex;

        private Regex templateRegex;

        /// <summary>
        /// Gets the compiled template of email.
        /// </summary>
        /// <value>The template.</value>
        public string Template { get; private set; }

        /// <summary>
        /// Gets the subject of email.
        /// </summary>
        /// <value>The subject.</value>
        public string Subject { get; private set; }


        /// <summary>
        /// Specifies the date email wanted to be sent.
        /// </summary>
        public DateTime? SendOn { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="culture">The culture (tr-TR / en-US).</param>
        /// <param name="dataFields">The data fields of email</param>

        /// <param name="dataFields">The data fields of email</param>

        public Email(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, string subject = "", string templateData = "")
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
        public void AddRecipient(string to)
        {
            toList.Add(to);
        }

        public void AddToQueue(string from, string fromName, string cc = "", string bcc = "",string template = "",string subject = "")
        {
            var queue = new EmailEntry()
            {
                EmailTo = toList,
                EmailFrom = from,
                ReplyTo = from,
                FromName = fromName,
                CC = cc,
                Bcc = bcc,
                Subject = subject,
                Body = template,
                UniqueID = null,
                QueueTime = DateTime.UtcNow,
                SendOn = SendOn
            };
            BackgroundJob.Enqueue<Jobs.Email.Email>(email => email.TransmitMail(queue));

        }

        public void AddToQueue(int recordId = 0, int tenantId = 0, string from = "", string fromName = "", string cc = "", string bcc = "", UserItem appUser = null, string fromEmail = "", string Name = "",string Subject = "",string Template = "")
        {
            if (string.IsNullOrEmpty(from))
            {
                from = "destek@primeapps.com";
                fromName = "PrimeApps.io";
            }

            if (appUser != null)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var pdbCtx = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                    var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();


                    using (TenantRepository tRepo = new TenantRepository(pdbCtx, _configuration))//, cacheHelper))
					{
                        var tenant = tRepo.Get(appUser.TenantId);
                        if (!string.IsNullOrEmpty(tenant.Setting?.MailSenderName) && !string.IsNullOrEmpty(tenant.Setting?.MailSenderEmail))
                        {
                            from = tenant.Setting.MailSenderEmail;
                            fromName = tenant.Setting.MailSenderName;
                        }
                    }
                }
            }

            var queue = new EmailEntry()
            {
                EmailTo = toList,
                EmailFrom = Regex.Replace(from, "^pre__", ""),
                ReplyTo = Regex.Replace(from, "^pre__", ""),
                FromName = fromName,
                CC = !string.IsNullOrEmpty(cc) ? Regex.Replace(cc, "^pre__", "") : cc,
                Bcc = !string.IsNullOrEmpty(bcc) ? Regex.Replace(bcc, "^pre__", "") : bcc,
                Subject = Subject,
                Body = Template,
                UniqueID = null,
                QueueTime = DateTime.UtcNow,
                SendOn = SendOn
            };
            BackgroundJob.Enqueue<Jobs.Email.Email>(email => email.TransmitMail(queue));

        }

    }
}
