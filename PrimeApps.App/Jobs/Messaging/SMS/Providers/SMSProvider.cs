using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using OfisimCRM.App.Jobs.Messaging.SMS.Providers;

namespace PrimeApps.App.Jobs.Messaging.SMS.Providers
{

    /// <summary>
    /// Standard abstract sms class for supported sms providers.
    /// </summary>
    public abstract class SMSProvider : IDisposable
    {
        protected string userName;
        protected string password;
        protected Uri apiAddress;


        public IList<Message> Messages { get; set; }
        internal string Alias { get; set; }
        internal DateTime? SendDate { get; set; }
        public Regex AcceptedPhoneNumberFormat { get; set; }

        public SMSProvider(string userName, string password)
        {
            this.Alias = "";
            this.SendDate = DateTime.UtcNow;
            this.Messages = new List<Message>();
            this.AcceptedPhoneNumberFormat = new Regex("[0-9 ]+");
            this.SetCredentials(userName, password);
        }

        public SMSProvider()
        {
            this.Alias = "";
            this.SendDate = DateTime.UtcNow;
            this.Messages = new List<Message>();
            this.AcceptedPhoneNumberFormat = new Regex("[0-9 ]+");
            this.SetCredentials("", "");
        }

        /// <summary>
        /// Sends prepared sms messages.
        /// </summary>
        /// <returns></returns>
        public abstract Task<SMSResponse> Send();

        public async Task<SMSResponse> Send(IList<Message> messages)
        {
            this.Messages = messages;
            return await Send();
        }

        public void SetCredentials(string userName, string password)
        {
            this.userName = userName;
            this.password = password;
        }

        /// <summary>
        /// Parses phone number by given regular expression.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public virtual string ParsePhoneNumber(string input)
        {
            if (String.IsNullOrWhiteSpace(input)) return null;

            Match match = AcceptedPhoneNumberFormat.Match(Regex.Replace(input, @"[^\d]", string.Empty));
            if (!match.Success) return String.Empty;
            return match.ToString();
        }

        /// <summary>
        /// Creates an instance of a supported sms provider class.s
        /// </summary>
        /// <param name="providerName"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public static SMSProvider Initialize(string providerName, string userName, string password)
        {
            SMSProvider newProvider;
            switch (providerName)
            {
                case "ajanssms":
                    newProvider = new AjansSMS(userName, password);
                    break;
                case "ttmesaj":
                    newProvider = new TTMesaj(userName, password);
                    break;
                case "verimorsms":
                    newProvider = new VerimorSMS(userName, password);
                    break;
                case "netgsmsms":
                    newProvider = new Netgsm(userName, password);
                    break;
                default:
                    throw new SMSProviderNotFoundException($"SMS Provider {providerName} is not implemented!");
            }
            return newProvider;
        }

        public void Dispose()
        {
        }

        /// <summary>
        /// Indicates that SMS Provider is not implemented or found.
        /// </summary>
        public class SMSProviderNotFoundException : Exception
        {
            public SMSProviderNotFoundException(string message) : base(message)
            {

            }
        }
    }
}
