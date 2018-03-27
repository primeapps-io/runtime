using PrimeApps.Model.Enums;
using System;
using System.Net;
using System.Net.Http;
using System.Net.Mail;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs.Messaging.EMail.Providers
{
    class SMTP : EMailProvider
    {
        public SMTP(string userName, string password) : base(userName, password)
        {

        }

        public override async Task<EMailResponse> Send()
        {
            NotificationStatus statusCode = NotificationStatus.Successful;

            string statusMessage = string.Empty;
            int mailQueue = 0;

            try
            {

                using (var smtpClient = new SmtpClient())
                {
                    smtpClient.Host = apiAddress.Host;
                    smtpClient.Port = apiAddress.Port;
                    smtpClient.EnableSsl = EnableSSL;
                    smtpClient.Credentials = new NetworkCredential(userName, password);
                    smtpClient.DeliveryFormat = SmtpDeliveryFormat.International;

                    foreach (var message in Messages)
                    {
                        /// Create Message
                        var mailMessage = new MailMessage();
                        mailMessage.Body = message.Body;
                        mailMessage.IsBodyHtml = true;
                        mailMessage.SubjectEncoding = Encoding.UTF8;
                        mailMessage.HeadersEncoding = Encoding.UTF8;
                        mailMessage.Subject = message.Subject;
                        mailMessage.From = new MailAddress(SenderEMail, Alias, Encoding.UTF8);
                        foreach (string recipient in message.Recipients)
                        {
                            mailMessage.To.Add(new MailAddress(recipient));
                        }

                        if (!string.IsNullOrWhiteSpace(message.AttachmentLink))
                        {
                            using (var httpClient = new HttpClient())
                            {
                                var content = await httpClient.GetAsync(new Uri(message.AttachmentLink));
                                var stream = await content.Content.ReadAsStreamAsync();
                                mailMessage.Attachments.Add(new Attachment(stream, message.AttachmentName, content.Content.Headers.ContentType.MediaType));
                            }
                        }

                        mailQueue++;
                        /// Send Message
                        await smtpClient.SendMailAsync(mailMessage);
                        smtpClient.SendCompleted += (sender, error) =>
                        {
                            mailMessage.Dispose();
                        };

#if DEBUG
                        // this code is only required for development environment where it is necessary to limit output with two mails per second.
                        if (mailQueue % 2 == 0)
                        {
                            await Task.Delay(1000);
                        }
#endif
                    }
                }
            }
            catch (Exception ex)
            {
                statusCode = NotificationStatus.ConnectionFailed;
                statusMessage = ex.Message;
            }

            return new EMailResponse()
            {
                Status = statusCode,
                Response = statusMessage
            };
        }

        public override void SetHost(string host, int port)
        {
            UriBuilder uriBuilder = new UriBuilder("smtp", host, port);
            this.apiAddress = uriBuilder.Uri;
        }

        public override void SetSender(string alias, string emailAddress)
        {
            this.Alias = alias;
            this.SenderEMail = emailAddress;
        }
    }
}
