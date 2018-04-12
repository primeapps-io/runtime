using Hangfire;
using PrimeApps.App.Helpers;
using PrimeApps.App.Jobs.QueueAttributes;
using PrimeApps.Model.Helpers;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using RecordHelper = PrimeApps.Model.Helpers.RecordHelper;
using PrimeApps.Model.Entities.Platform.Identity;
using PrimeApps.App.Jobs.Messaging;

namespace PrimeApps.App.Jobs.Email
{
    /// <summary>
    /// Schedules and send email messages application wide.
    /// </summary>
    public class Email
    {
        public Email()
        {
            // No DI required for this class since it only uses nhibernate and smtp provider.
        }

        public static object Messaging { get; internal set; }

        /// <summary>
        /// Checks database for new email records and transmits it if there is new one.
        /// </summary>
        /// <returns></returns>
        [EmailQueue, AutomaticRetry(Attempts = 0)]
        public bool TransmitMail(EmailEntry mail)
        {
            bool status = false;

            // get a record by the queue algorithm from database.

            if (mail != null)
            {
                // create smtp client and mail message objects
                SmtpClient smtpClient;
                MailMessage myMessage;
                var smtpHost = "EmailSMTPHost";
                var smtpPort = "EmailSMTPPort";
                var smtpUser = "EmailSMTPUser";
                var smtpPassword = "EmailSMTPPassword";
                var enableSsl = bool.Parse(ConfigurationManager.AppSettings["EmailSMTPEnableSsl"]);

                if (bool.Parse(ConfigurationManager.AppSettings["TestMode"]))
                {
                    smtpHost = "EmailSMTPHostTest";
                    smtpPort = "EmailSMTPPortTest";
                    smtpUser = "EmailSMTPUserTest";
                    smtpPassword = "EmailSMTPPasswordTest";
                }
                // get configuration settings from appsetting and apply them.
                smtpClient = new SmtpClient(ConfigurationManager.AppSettings[smtpHost], int.Parse(ConfigurationManager.AppSettings[smtpPort]))
                {
                    UseDefaultCredentials = false,
                    // set credentials
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings[smtpUser], ConfigurationManager.AppSettings[smtpPassword]),
                    DeliveryFormat = SmtpDeliveryFormat.International,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = enableSsl
                };

                if (string.IsNullOrWhiteSpace(mail.EmailTo))
                    return true;

                // generate email message
                myMessage = new MailMessage(new MailAddress(mail.EmailFrom, mail.FromName), new MailAddress(mail.EmailTo))
                {
                    Subject = mail.Subject,
                    Body = mail.Body,
                    IsBodyHtml = true
                };

                if (!string.IsNullOrWhiteSpace(mail.CC))
                {
                    var ccList = mail.CC.Split(',');
                    foreach (var cc in ccList)
                    {
                        myMessage.CC.Add(cc);
                    }
                }

                if (!string.IsNullOrWhiteSpace(mail.Bcc))
                {
                    var bccList = mail.Bcc.Split(',');

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

        [EmailQueue, AutomaticRetry(Attempts = 0)]
        public bool TransmitMail(EmailEntry email, int tenantId, int moduleId, int recordId, bool addRecordSummary = true)
        {
            bool status = false;

            if (email != null)
            {
                // create smtp client and mail message objects
                SmtpClient smtpClient;
                MailMessage myMessage;
                var smtpHost = "EmailSMTPHost";
                var smtpPort = "EmailSMTPPort";
                var smtpUser = "EmailSMTPUser";
                var smtpPassword = "EmailSMTPPassword";
                var enableSsl = bool.Parse(ConfigurationManager.AppSettings["EmailSMTPEnableSsl"]);

                if (bool.Parse(ConfigurationManager.AppSettings["TestMode"]))
                {
                    smtpHost = "EmailSMTPHostTest";
                    smtpPort = "EmailSMTPPortTest";
                    smtpUser = "EmailSMTPUserTest";
                    smtpPassword = "EmailSMTPPasswordTest";
                }
                // get configuration settings from appsetting and apply them.
                smtpClient = new SmtpClient(ConfigurationManager.AppSettings[smtpHost], int.Parse(ConfigurationManager.AppSettings[smtpPort]))
                {
                    UseDefaultCredentials = false,
                    // set credentials
                    Credentials = new NetworkCredential(ConfigurationManager.AppSettings[smtpUser], ConfigurationManager.AppSettings[smtpPassword]),
                    DeliveryFormat = SmtpDeliveryFormat.International,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = enableSsl
                };

                // parse subject and body
                var subject = AsyncHelpers.RunSync(() => ParseDynamicContent(email.Subject, tenantId, moduleId, recordId, false));
                var body = AsyncHelpers.RunSync(() => ParseDynamicContent(email.Body, tenantId, moduleId, recordId, addRecordSummary));

                // generate email message
                myMessage = new MailMessage(new MailAddress(email.EmailFrom, email.FromName), new MailAddress(email.EmailTo))
                {
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };

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

        private static async Task<string> ParseDynamicContent(string content, int tenantId, int moduleId, int recordId, bool addRecordSummary = true)
        {
            var pattern = new Regex(@"{(.*?)}");
            var contentFields = new List<string>();
            var matches = pattern.Matches(content);
            PlatformUser subscriber = null;

            using (var platformDBContext = new PlatformDBContext())
            using (var platformUserRepository = new PlatformUserRepository(platformDBContext))
            {
                subscriber = await platformUserRepository.GetWithTenant(tenantId);
            }

            foreach (object match in matches)
            {
                string fieldName = match.ToString().Replace("{", "").Replace("}", "");

                if (!contentFields.Contains(fieldName))
                {
                    contentFields.Add(fieldName);
                }
            }


            using (var databaseContext = new TenantDBContext(tenantId))
            using (var moduleRepository = new ModuleRepository(databaseContext))
            using (var picklistRepository = new PicklistRepository(databaseContext))
            using (var recordRepository = new RecordRepository(databaseContext))
            {
                var module = await moduleRepository.GetById(moduleId);
                var lookupModules = await RecordHelper.GetLookupModules(module, moduleRepository);
                var record = recordRepository.GetById(module, recordId, false, lookupModules);

                if (!record.IsNullOrEmpty())
                {
                    record = await RecordHelper.FormatRecordValues(module, record, moduleRepository, picklistRepository, subscriber.Tenant.Language, subscriber.Culture, 180, lookupModules, true);

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

                        var fields = module.Fields.Where(x => x.DisplayDetail && x.Validation != null && x.Validation.Required.HasValue && x.Validation.Required.Value && (x.Permissions == null || x.Permissions.Count < 1));

                        if (module.Name == "izinler" && !record["calisan.id"].IsNullOrEmpty())
                        {
                            var tarih = (string)record["baslangic_tarihi"] + " - " + record["bitis_tarihi"] + " / " + record["hesaplanan_alinacak_toplam_izin"] + " " + "Gün";

                            recordTable += recordRow.Replace("{label}", "Adı Soyadı").Replace("{value}", record["calisan.ad_soyad"].ToString());
                            recordTable += recordRow.Replace("{label}", "Unvanı").Replace("{value}", record["calisan.unvan"].ToString());
                            recordTable += recordRow.Replace("{label}", "Departman").Replace("{value}", record["calisan.departman"].ToString());
                            recordTable += recordRow.Replace("{label}", "İzin Türü").Replace("{value}", record["izin_turu.adi"].ToString());
                            recordTable += recordRow.Replace("{label}", "Tarih").Replace("{value}", tarih);
                            recordTable += recordRow.Replace("{label}", "Açıklama").Replace("{value}", record["izin_turu.aciklama"].ToString());

                        }
                        else
                        {
                            foreach (var field in fields)
                            {
                                if (!record[field.Name].IsNullOrEmpty())
                                    recordTable += recordRow.Replace("{label}", field.LabelTr).Replace("{value}", record[field.Name].ToString());

                            }

                        }

                        content = content.Replace("[[recordTable]]", recordTable);
                    }
                    else
                    {
                        content = content.Replace("[[recordTable]]", "");
                    }
                }
            }

            return content;
        }
    }
}