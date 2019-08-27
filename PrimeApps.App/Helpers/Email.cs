using Hangfire;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Jobs.Messaging;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Resources;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

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
        public IList<string> toList = new List<string>();

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
        public Email(EmailResource resourceType, string culture, Dictionary<string, string> dataFields, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, int appId, UserItem appUser)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;

            var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
            previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

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

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var tdbCtx = scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (TemplateRepository tRepo = new TemplateRepository(tdbCtx, configuration))
                {

                    tRepo.CurrentUser = new CurrentUser { TenantId = previewMode == "app" ? appUser.AppId : appUser.TenantId, UserId = appUser.Id, PreviewMode = previewMode };

                    templateEntity = tRepo.GetByCode(resourceTypeName, language);
                }

                switch (appId)
                {
                    case 1:
                        appUrl = "http://www.ofisim.com/mail/crm/logo.png";
                        appCodeUrl = "http://www.ofisim.com/crm/";
                        appName = "Ofisim CRM";
                        appColor = "2560f6";
                        socialMediaIcons = "true";
                        footer = "Ofisim.com";
                        appLogo = "";
                        break;
                    case 2:
                        appUrl = "http://www.ofisim.com/mail/kobi/logo.png";
                        appCodeUrl = "http://www.ofisim.com/kobi/";
                        appName = "Ofisim KOBİ";
                        appColor = "20cb9a";
                        socialMediaIcons = "true";
                        footer = "Ofisim.com";
                        appLogo = "";
                        break;
                    case 3:
                        appUrl = "http://www.ofisim.com/mail/asistan/logo.png";
                        appCodeUrl = "http://www.ofisim.com/asistan/";
                        appName = "Ofisim ASİSTAN";
                        appColor = "ef604e";
                        socialMediaIcons = "true";
                        footer = "Ofisim.com";
                        appLogo = "";
                        break;
                    case 4:
                        appUrl = "http://www.ofisim.com/mail/ik/logo.png";
                        appCodeUrl = "http://www.ofisim.com/ik/";
                        appName = "Ofisim İK";
                        appColor = "454191";
                        socialMediaIcons = "true";
                        footer = "Ofisim.com";
                        appLogo = "";
                        break;
                    case 5:
                        appUrl = "http://www.ofisim.com/mail/cagri/logo.png";
                        appCodeUrl = "http://www.ofisim.com/cagri/";
                        appName = "Ofisim ÇAĞRI";
                        appColor = "79a7fd";
                        socialMediaIcons = "true";
                        footer = "Ofisim.com";
                        appLogo = "";
                        break;
                    default:
                        appUrl = "http://www.ofisim.com/mail/crm/logo.png";
                        appCodeUrl = "http://www.ofisim.com/crm/";
                        appName = "Perapol APP";
                        appColor = "2560f6";
                        socialMediaIcons = "true";
                        footer = "Perapole";
                        appLogo = "";
                        break;

                }

                var pdbCtx = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();


                using (TenantRepository tRepo = new TenantRepository(pdbCtx, configuration))//, cacheHelper))
                {
                    var instance = tRepo.Get(appUser.TenantId);

                    if (!string.IsNullOrEmpty(instance.Setting?.MailSenderName) && !string.IsNullOrEmpty(instance.Setting?.MailSenderEmail))
                    {
                        appUrl = instance.Setting.Logo;
                        appCodeUrl = "#";
                        appName = instance.Setting.MailSenderName;
                        socialMediaIcons = "none";
                        footer = instance.Setting.MailSenderName;

                        if (instance.Setting.MailSenderEmail.Contains("@etiya.com"))
                        {
                            appLogo = "none";
                        }
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
            int startIndex = tmpl.IndexOf("{{F}}");
            int lastIndex = tmpl.IndexOf("{{/F}}");

            if (startIndex > -1 && lastIndex > -1 && string.Equals(socialMediaIcons, "none"))
                tmpl = tmpl.Remove(startIndex, lastIndex - startIndex + 6);

            else if (string.Equals(socialMediaIcons, "true"))
            {
                tmpl = tmpl.Replace("{{F}}", "");
                tmpl = tmpl.Replace("{{/F}}", "");
            }
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
        public Email(string subject, string templateData, IConfiguration configuration)
        {
            _configuration = configuration;
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
        public void AddToQueue(int recordId = 0, int tenantId = 0, string from = "", string fromName = "", string cc = "", string bcc = "", UserItem appUser = null, string fromEmail = "", string Name = "")
        {
            if (string.IsNullOrEmpty(from))
            {
                from = !string.IsNullOrEmpty(fromEmail) ? fromEmail : "app@primeapps.io";
                fromName = !string.IsNullOrEmpty(Name) ? Name : "PrimeApps";
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

        public void AddToQueue(string from, string fromName, string cc = "", string bcc = "")
        {
            var queue = new EmailEntry()
            {
                EmailTo = toList,
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
            BackgroundJob.Enqueue<Jobs.Email.Email>(email => email.TransmitMail(queue));

        }

        /// <summary>
        /// Writes email(s) into the database in a stateless session context.
        /// </summary>
        public async System.Threading.Tasks.Task AddToQueueAsync(int tenantId, int moduleId, int recordId, string from = "", string fromName = "", string cc = "", string bcc = "", UserItem appUser = null, bool addRecordSummary = true)
        {
            from = "admin@perapole.com";
            fromName = "Perapole";

            if (appUser != null)
            {
                using (var scope = _serviceScopeFactory.CreateScope())
                {
                    var pdbCtx = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                    var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();
                    var databaseContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();

                    using (AppDraftRepository platforcm = new AppDraftRepository(databaseContext, _configuration))
                    {
                        var app = await platforcm.Get(appUser.AppId);

                        from = app.Setting.MailSenderEmail;
                        fromName = app.Setting.MailSenderName;

                    }

                    using (TenantRepository tRepo = new TenantRepository(pdbCtx, _configuration))//, cacheHelper))
                    {
                        var instance = tRepo.Get(appUser.TenantId);
                        if (!string.IsNullOrEmpty(instance.Setting?.MailSenderName) && !string.IsNullOrEmpty(instance.Setting?.MailSenderEmail))
                        {
                            from = instance.Setting.MailSenderEmail;
                            fromName = instance.Setting.MailSenderName;
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
            BackgroundJob.Enqueue<Jobs.Email.Email>(email => email.TransmitMail(queue, tenantId, moduleId, recordId, appUser, addRecordSummary));

        }

    }
}