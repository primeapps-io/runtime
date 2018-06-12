using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Hangfire;
using PrimeApps.App.Jobs.Messaging;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Common.Resources;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Helpers
{
    /// <summary>
    /// This class contains email functions and template definitions.
    /// We store new emails directly inside of database, crmEmailQueue table.
    /// Worker roles are fetching records from database and sending them to the receiver email addresses.
    /// Email system works with a queue algorithm.
    /// <summary>
    /// Class Email.
    /// </summary>
    public class Email
    {
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
        public Email(EmailResource resourceType, string culture, Dictionary<string, string> dataFields, int AppId = 1, UserItem AppUser = null)
        {
            string tmpl = "",
                   appUrl = "",
                   appName = "",
                   appCodeUrl = "",
                   appColor = "",
                appLogo = "",
                   socialMediaIcons = "",
                   footer = "",
                   resourceTypeName = "";


            dataRegex = new Regex(dataPattern);
            templateRegex = new Regex(templatePattern);
            resourceTypeName = GetResourceTypeName(resourceType);

            LanguageType language = culture.Contains("tr") ? LanguageType.Tr : LanguageType.En;
            Template templateEntity;
            using (TenantDBContext tdbCtx = new TenantDBContext(AppUser.TenantId))
            using (TemplateRepository tRepo = new TemplateRepository(tdbCtx))
            {
                templateEntity = tRepo.GetByCode(resourceTypeName, language);
            }

            switch (AppId)
            {
                case 1:
                    appUrl = "http://www.ofisim.com/mail/crm/logo.png";
                    appCodeUrl = "http://www.ofisim.com/crm/";
                    appName = "Ofisim CRM";
                    appColor = "2560f6";
                    socialMediaIcons = "true";
                    footer = "Ofisim.com";
                    appLogo = "true";
                    break;
                case 2:
                    appUrl = "http://www.ofisim.com/mail/kobi/logo.png";
                    appCodeUrl = "http://www.ofisim.com/kobi/";
                    appName = "Ofisim KOBİ";
                    appColor = "20cb9a";
                    socialMediaIcons = "true";
                    footer = "Ofisim.com";
                    appLogo = "true";
                    break;
                case 3:
                    appUrl = "http://www.ofisim.com/mail/asistan/logo.png";
                    appCodeUrl = "http://www.ofisim.com/asistan/";
                    appName = "Ofisim ASİSTAN";
                    appColor = "ef604e";
                    socialMediaIcons = "true";
                    footer = "Ofisim.com";
                    appLogo = "true";
                    break;
                case 4:
                    appUrl = "http://www.ofisim.com/mail/ik/logo.png";
                    appCodeUrl = "http://www.ofisim.com/ik/";
                    appName = "Ofisim İK";
                    appColor = "454191";
                    socialMediaIcons = "true";
                    footer = "Ofisim.com";
                    appLogo = "true";
                    break;
                case 5:
                    appUrl = "http://www.ofisim.com/mail/cagri/logo.png";
                    appCodeUrl = "http://www.ofisim.com/cagri/";
                    appName = "Ofisim ÇAĞRI";
                    appColor = "79a7fd";
                    socialMediaIcons = "true";
                    footer = "Ofisim.com";
                    appLogo = "true";
                    break;
                default:
                    appUrl = "http://www.ofisim.com/mail/crm/logo.png";
                    appCodeUrl = "http://www.ofisim.com/crm/";
                    appName = "Ofisim CRM";
                    appColor = "2560f6";
                    socialMediaIcons = "true";
                    footer = "Ofisim.com";
                    appLogo = "true";
                    break;

            }

            if (AppUser != null)
            {
                using (PlatformDBContext pdbCtx = new PlatformDBContext())
                using (TenantRepository tRepo = new TenantRepository(pdbCtx))
                {
                    var instance = tRepo.Get(AppUser.TenantId);
                    if (!string.IsNullOrEmpty(instance.Setting.MailSenderName) && !string.IsNullOrEmpty(instance.Setting.MailSenderEmail))
                    {
                        appUrl = TenantRepository.GetLogoUrl(instance.Setting.Logo);
                        appCodeUrl = "#";
                        appName = instance.Setting.MailSenderName;
                        socialMediaIcons = "none";
                        footer = instance.Setting.MailSenderName;
                        //TODO Removed
                        /*if (instance.MailSenderEmail.Contains("@etiya.com"))
                        {
                            appLogo = "none";
                        }*/
                    }
                }
            }


            tmpl = templateEntity.Content;
            tmpl = tmpl.Replace("{{URL}}", appUrl);
            tmpl = tmpl.Replace("{{APP_URL}}", appCodeUrl);
            tmpl = tmpl.Replace("{{APP_COLOR}}", appColor);
            tmpl = tmpl.Replace("{{SOCIAL_ICONS}}", socialMediaIcons);
            tmpl = tmpl.Replace("{{FOOTER}}", footer);
            tmpl = tmpl.Replace("{{APP_LOGO}}", appLogo);
            dataFields.Add("appName", appName);

            // fill this value with data parameters if any of them in it.
            tmpl = FillData(tmpl, dataFields);


            this.Template = tmpl;
            this.Subject = templateEntity.Subject; /*lcl.GetString("Subject");*/


            /// fill subject with data if there are any data fields.
            this.Subject = FillData(this.Subject, dataFields);


        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Email"/> class with subject and template with data inside.
        /// </summary>
        /// <param name="resourceType">Type of the resource.</param>
        /// <param name="culture">The culture (tr-TR / en-US).</param>
        /// <param name="dataFields">The data fields of email</param>
        public Email(string subject, string templateData)
        {
            this.Template = templateData;
            this.Subject = subject;
        }

        /// <summary>
        /// Translates email templates.
        /// </summary>
        /// <param name="templateText">Email template</param>
        /// <param name="localization">The localization data.</param>
        /// <param name="dataFields">The data fields.</param>
        /// <returns>System.String.</returns>
        private string TranslateTemplate(string templateText, Localization localization)
        {
            string key = "",
              value = "",
              placeholder = "",
              tmpl = templateText;

            // get parameters by regex.
            var matches = templateRegex.Matches(tmpl);

            foreach (var match in matches)
            {
                // convert match to string and get the key out of it.
                placeholder = match.ToString();
                key = placeholder.Replace("{", "").Replace("}", "");

                // try to get localized key value.
                value = localization.GetString(key);

                // replace the placeholder with value in the template.
                tmpl = tmpl.Replace(placeholder, value);
            }

            return tmpl;
        }

        /// <summary>
        /// Fills the data into translated template fields.
        /// </summary>
        /// <param name="templateText">The template text.</param>
        /// <param name="dataFields">The data fields.</param>
        /// <returns>System.String.</returns>
        private string FillData(string templateText, Dictionary<string, string> dataFields)
        {
            string key = "",
                   value = "",
                   placeholder = "",
                   tmpl = templateText;
            var matches = dataRegex.Matches(tmpl);
            foreach (var match in matches)
            {
                placeholder = match.ToString();
                key = placeholder.Replace("{:", "").Replace("}", "");

                if (dataFields.TryGetValue(key, out value))
                {
                    tmpl = tmpl.Replace(placeholder, value);
                }

            }
            return tmpl;
        }

        private string GetResourceTypeName(EmailResource value)
        {
            FieldInfo fi = value.GetType().GetField(value.ToString());

            ResourceNameAttribute[] attributes =
                (ResourceNameAttribute[])fi.GetCustomAttributes(
                typeof(ResourceNameAttribute),
                false);

            if (attributes == null ||
                attributes.Length == 0)
                throw new Exception("Resource Name is not defined.");


            return attributes[0].ResourceName;
        }
        /// <summary>
        /// Adds the email address to the recipients of email.
        /// </summary>
        /// <param name="to">To.</param>
        public void AddRecipient(string to)
        {
            toList.Add(to);
        }

        /// <summary>
        /// Writes email(s) into the database in a stateless session context.
        /// Warning: Do not use this method in a transaction, instead use AddToQueue(ISession session) method to add it.
        /// </summary>
        public void AddToQueue(int recordId = 0, int tenantId = 0, string from = "", string fromName = "", string cc = "", string bcc = "", UserItem appUser = null)
        {
            from = "destek@ofisim.com";
            fromName = "Ofisim.com";

            if (appUser != null)
            {
                using (PlatformDBContext pdbCtx = new PlatformDBContext())
                using (TenantRepository tRepo = new TenantRepository(pdbCtx))
                {
                    var tenant = tRepo.Get(appUser.TenantId);
                    if (!string.IsNullOrEmpty(tenant.Setting.MailSenderName) && !string.IsNullOrEmpty(tenant.Setting.MailSenderEmail))
                    {
                        from = tenant.Setting.MailSenderEmail;
                        fromName = tenant.Setting.MailSenderName;
                    }
                }
            }

            foreach (string to in toList)
            {
                var queue = new EmailEntry()
                {
                    EmailTo = Regex.Replace(to, "^pre__", ""),
                    EmailFrom = Regex.Replace(from, "^pre__", ""),
                    ReplyTo = Regex.Replace(from, "^pre__", ""),
                    FromName = fromName,
                    CC = Regex.Replace(cc, "^pre__", ""),
                    Bcc = Regex.Replace(bcc, "^pre__", ""),
                    Subject = Subject,
                    Body = Template,
                    UniqueID = null,
                    QueueTime = DateTime.UtcNow,
                    SendOn = SendOn
                };
                BackgroundJob.Schedule<Jobs.Email.Email>(email => email.TransmitMail(queue), TimeSpan.FromSeconds(5));
            }
        }

		public void AddToQueue(string from, string fromName, string cc = "", string bcc = "")
		{
			foreach (string to in toList)
			{
				var queue = new EmailEntry()
				{
					EmailTo = to,
					EmailFrom = from,
					ReplyTo = from,
					FromName = fromName,
					CC = cc,
					Bcc = bcc,
					Subject = Subject,
					Body = Template,
					UniqueID = null,
					QueueTime = DateTime.UtcNow,
					SendOn = SendOn
				};
				BackgroundJob.Schedule<Jobs.Email.Email>(email => email.TransmitMail(queue), TimeSpan.FromSeconds(5));
			}
		}

		/// <summary>
		/// Writes email(s) into the database in a stateless session context.
		/// </summary>
		public void AddToQueue(int tenantId, int moduleId, int recordId, string from = "", string fromName = "", string cc = "", string bcc = "", UserItem appUser = null, bool addRecordSummary = true)
        {
            from = "destek@ofisim.com";
            fromName = "Ofisim.com";

            if (appUser != null)
            {
                using (PlatformDBContext pdbCtx = new PlatformDBContext())
                using (TenantRepository tRepo = new TenantRepository(pdbCtx))
                {
                    var instance = tRepo.Get(appUser.TenantId);
                    if (!string.IsNullOrEmpty(instance.Setting.MailSenderName) && !string.IsNullOrEmpty(instance.Setting.MailSenderEmail))
                    {
                        //TODO Removed
                        /*if (!string.IsNullOrEmpty(fromEmail))
                            from = fromEmail;
                        else*/
                            from = instance.Setting.MailSenderEmail;
                        fromName = instance.Setting.MailSenderName;
                    }
                }
            }

            foreach (string to in toList)
            {
                var queue = new EmailEntry()
                {
                    EmailTo = Regex.Replace(to, "^pre__", ""),
                    EmailFrom = Regex.Replace(from, "^pre__", ""),
                    ReplyTo = Regex.Replace(from, "^pre__", ""),
                    FromName = fromName,
                    CC = Regex.Replace(cc, "^pre__", ""),
                    Bcc = Regex.Replace(bcc, "^pre__", ""),
                    Subject = Subject,
                    Body = Template,
                    UniqueID = null,
                    QueueTime = DateTime.UtcNow,
                    SendOn = SendOn
                };
                BackgroundJob.Schedule<Jobs.Email.Email>(email => email.TransmitMail(queue, tenantId, moduleId, recordId, addRecordSummary), TimeSpan.FromSeconds(5));
            }
        }

    }
}