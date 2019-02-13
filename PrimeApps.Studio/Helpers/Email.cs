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
                var enableSsl = bool.Parse(_configuration.GetSection("AppSettings")["EmailSMTPEnableSsl"]);

                // get configuration settings from appsetting and apply them.
                smtpClient = new SmtpClient(_configuration.GetSection("AppSettings")[smtpHost], int.Parse(_configuration.GetSection("AppSettings")[smtpPort]))
                {
                    UseDefaultCredentials = false,
                    // set credentials
                    Credentials = new NetworkCredential(_configuration.GetSection("AppSettings")[smtpUser], _configuration.GetSection("AppSettings")[smtpPassword]),
                    DeliveryFormat = SmtpDeliveryFormat.International,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    EnableSsl = enableSsl
                };

                // transmit it.
                smtpClient.Send(mail);

                // set status to true
                status = true;
            }

            // return status.
            return status;
        }
       
    }
}
