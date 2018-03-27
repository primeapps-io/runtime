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
    /// SMS Provider implementation for AjansSMS.
    /// </summary>
    class AjansSMS : SMSProvider
    {
        public AjansSMS(string userName = "", string password = "") : base(userName, password)
        {
            this.apiAddress = new Uri("http://g.ajanswebsms.com");
            this.AcceptedPhoneNumberFormat = new Regex(@"(5)[0-9][0-9][0-9]([0-9]){6}$");
        }

        const string smsTemplate = @"<MultiTextSMS>
                                    <UserName>{userName}</UserName>
                                    <PassWord>{password}</PassWord>
                                    <Action>13</Action>
                                    <Messages>
                                        {messages}
                                    </Messages>
                                    <Originator>{originator}</Originator>
                                    <SDate></SDate>
                                   </MultiTextSMS>";


        public override async Task<SMSResponse> Send()
        {
            StringBuilder smsXML = new StringBuilder(smsTemplate);
            smsXML.Replace("{userName}", this.userName);
            smsXML.Replace("{password}", this.password);
            smsXML.Replace("{originator}", this.Alias);

            StringBuilder messagesXML = new StringBuilder();
            StringBuilder messageXML = new StringBuilder();
            foreach (Message msg in Messages)
            {
                messageXML.Append("<Message>");
                messageXML.AppendFormat("<Mesgbody>{0}</Mesgbody>", msg.Body);
                foreach (string recipient in msg.Recipients)
                {
                    messageXML.AppendFormat("<Number>{0}</Number>", recipient);
                }
                messageXML.Append("</Message>");
                messagesXML.Append(messageXML);
                messageXML.Clear();
            }
            smsXML.Replace("{messages}", messagesXML.ToString());
            return await SendHTTP(smsXML.ToString());
        }

        private async Task<SMSResponse> SendHTTP(string xmlMessages)
        {
            NotificationStatus status;
            string sWebPage = "";
            try
            {

                WebClient wUpload = new WebClient();
                wUpload.Proxy = null;
                Byte[] bPostArray = Encoding.UTF8.GetBytes(xmlMessages);
                Byte[] bResponse = await wUpload.UploadDataTaskAsync(this.apiAddress, "POST", bPostArray);
                Char[] sReturnChars = Encoding.UTF8.GetChars(bResponse);
                sWebPage = new string(sReturnChars);

                switch (sWebPage)
                {
                    case "01":
                        status = NotificationStatus.InvalidUserNamePassword;
                        break;
                    case "02":
                        status = NotificationStatus.InsufficentCredits;
                        break;
                    case "03":
                        status = NotificationStatus.OtherError;
                        break;
                    case "04":
                        status = NotificationStatus.OtherError;
                        break;
                    case "05":
                        status = NotificationStatus.InvalidXML;
                        break;
                    case "06":
                        status = NotificationStatus.InvalidAlias;
                        break;
                    case "07":
                        status = NotificationStatus.OtherError;
                        break;
                    case "08":
                        status = NotificationStatus.OtherError;
                        break;
                    case "09":
                        status = NotificationStatus.OtherError;
                        break;
                    case "10":
                        status = NotificationStatus.OtherError;
                        break;
                    default:
                        status = NotificationStatus.Successful;
                        break;
                }
            }
            catch
            {
                return new SMSResponse()
                {
                    Response = "",
                    Status = NotificationStatus.SystemError
                };
            }
            return new SMSResponse()
            {
                Response = sWebPage,
                Status = status
            };
        }
    }
}
