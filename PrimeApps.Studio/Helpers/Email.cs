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
        private IList<string> toList = new List<string>();

        public DateTime? SendOn { get; set; }

        public Email(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, string subject = "", string templateData = "")
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
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
