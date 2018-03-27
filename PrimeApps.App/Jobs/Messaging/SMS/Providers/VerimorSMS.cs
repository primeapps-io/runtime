using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace PrimeApps.App.Jobs.Messaging.SMS.Providers
{

    /// <summary>
    /// SMS Provider implementation for Verimor.
    /// </summary>
    class VerimorSMS : SMSProvider
    {
        public VerimorSMS(string userName = "", string password = "") : base(userName, password)
        {
            this.apiAddress = new Uri("http://sms.verimor.com.tr/v2/send.json");
            this.AcceptedPhoneNumberFormat = new Regex(@"(90)(5)[0-9][0-9][0-9]([0-9]){6}$");
        }


        public override async Task<SMSResponse> Send()
        {
            var sendMsg = new SmsRequest();
            sendMsg.username = this.userName;
            sendMsg.password = this.password;
            sendMsg.source_addr = this.Alias;
            bool turkishCharMatch = false;
            IList<Message> messages = new List<Message>();

            foreach (Messaging.Message message in this.Messages)
            {
                foreach (var recipient in message.Recipients)
                {
                    messages.Add(new Message
                    {
                        dest = recipient,
                        msg = message.Body
                    });
                }

                /// Not - 1: datacoding = 0 veya datacoding = 1 gönderimlerde aşağıdaki karakterler 2 karakter sayılır.
                /// ^ {} \ [ ] ~ | €
                /// Not-2: Sadece(Ş ş Ğ ğ ç ı İ) harfleri Türkçe olarak kabul edilir ve datacoding=1 olarak gönderilmelidir.
                /// Diğer Türkçe karakterleri (Ö ö U ü Ç) datacoding=0 olarak gönderebilirsiniz.
                turkishCharMatch = turkishCharMatch ? true : message.Body.IndexOfAny("ŞşĞğÇçİı".ToCharArray()) != -1;
            }
            sendMsg.datacoding = turkishCharMatch ? "1" : "0";
            sendMsg.messages = messages.ToArray();
            return await SendHTTP(sendMsg);
        }

        public override string ParsePhoneNumber(string input)
        {
            // adding missing numbers before validate number
            var regexFirstChars = this.AcceptedPhoneNumberFormat.ToString().Substring(0, 3);

            if (regexFirstChars.Contains("9"))
            {
                if (input.Length == 10 && !input.StartsWith("90"))
                    input = "90" + input;
                else if (input.Length == 11 && !input.StartsWith("9"))
                    input = "9" + input;
            }

            return base.ParsePhoneNumber(input);
        }

        private async Task<SMSResponse> SendHTTP(SmsRequest messages)
        {
            NotificationStatus status;
            string sWebPage = "";
            try
            {
                string payload = JsonConvert.SerializeObject(messages);

                WebClient wc = new WebClient();
                wc.Headers["Content-Type"] = "application/json";
                wc.Encoding = Encoding.UTF8;
                sWebPage = wc.UploadString(this.apiAddress, payload);
                status = NotificationStatus.Successful;
            }
            catch
            {
                return new SMSResponse()
                {
                    Response = "",
                    Status = NotificationStatus.InvalidUserNamePassword
                };
            }
            return new SMSResponse()
            {
                Response = sWebPage,
                Status = status
            };
        }

        class Message
        {
            public string msg { get; set; }
            public string dest { get; set; }

            public Message() { }

            public Message(string msg, string dest)
            {
                this.msg = msg;
                this.dest = dest;
            }
        }
        class SmsRequest
        {
            public string username { get; set; }
            public string password { get; set; }
            public string source_addr { get; set; }
            public string datacoding { get; set; }
            public Message[] messages { get; set; }
        }
    }
}
