using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs.Messaging.EMail.Providers
{

    /// <summary>
    /// Standard abstract email class for supported email providers.
    /// </summary>
    public abstract class EMailProvider : IDisposable
    {
        protected string userName;
        protected string password;
        protected Uri apiAddress;


        public IList<Message> Messages { get; set; }
        internal string Alias { get; set; }
        internal string SenderEMail { get; set; }
        internal DateTime? SendDate { get; set; }
        internal bool EnableSSL { get; set; }

        public EMailProvider(string userName, string password)
        {
            this.Alias = "";
            this.SendDate = DateTime.UtcNow;
            this.SetCredentials(userName, password);
        }

        public EMailProvider()
        {
            this.Alias = "";
            this.SendDate = DateTime.UtcNow;
            this.Messages = new List<Message>();
            this.SetCredentials("", "");
        }

        /// <summary>
        /// Sends prepared sms messages.
        /// </summary>
        /// <returns></returns>
        public abstract Task<EMailResponse> Send();

        public async Task<EMailResponse> Send(IList<Message> messages)
        {
            this.Messages = messages;
            return await Send();
        }

        /// <summary>
        /// Sets credentials to connect to provider.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        public void SetCredentials(string userName, string password)
        {
            this.userName = userName;
            this.password = password;
        }

        /// <summary>
        /// Sets host and port for the provider.
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public abstract void SetHost(string host, int port);
        /// <summary>
        /// Sets sender alias and email address
        /// </summary>
        /// <param name="host"></param>
        /// <param name="port"></param>
        public abstract void SetSender(string alias, string emailAddress);

        /// <summary>
        /// Creates an instance of a supported email provider class.
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static EMailProvider Initialize(string providerName, string userName, string password)
        {
            EMailProvider newProvider;
            switch (providerName)
            {
                case "smtp":
                    newProvider = new SMTP(userName, password);
                    break;
                default:
                    throw new EMailProviderNotFoundException($"EMail Provider {providerName} is not implemented!");
            }
            return newProvider;
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Indicates that EMail Provider is not implemented or found.
        /// </summary>
        public class EMailProviderNotFoundException : Exception
        {
            public EMailProviderNotFoundException(string message) : base(message)
            {

            }
        }
    }
}
