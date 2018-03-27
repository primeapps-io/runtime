using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace PrimeApps.App.Jobs.Messaging.SMS.Providers
{
    class TTMesaj : SMSProvider
    {

        Regex resultRegex = new Regex(@"<sendNtoNSMSResult>(.+?)<\/sendNtoNSMSResult>");
        const string apiOperationAddress = "sendNtoNSMS";

        const string smsTemplate = @"<soap:Envelope xmlns:soap=""http://www.w3.org/2003/05/soap-envelope"" xmlns:tem=""http://tempuri.org/"">
   <soap:Header/>
   <soap:Body>
      <tem:sendNtoNSMS>
         <!--Optional:-->
         <tem:username>{userName}</tem:username>
         <!--Optional:-->
         <tem:password>{password}</tem:password>
         <!--Optional:-->
         <tem:xmData><![CDATA[{messages}]]></tem:xmData>
         <!--Optional:-->
         <tem:origin>{origin}</tem:origin>
         <!--Optional:-->
         <tem:sd>0</tem:sd>
         <!--Optional:-->
         <tem:ed>0</tem:ed>
      </tem:sendNtoNSMS>
   </soap:Body>
</soap:Envelope>";

        public TTMesaj(string userName, string password) : base(userName, password)
        {
            this.apiAddress = new Uri("http://ws.ttmesaj.com/service1.asmx");
            this.AcceptedPhoneNumberFormat = new Regex(@"(9?)(0?)(5)[0-9][0-9][0-9]([0-9]){6}$");
        }



        public override async Task<SMSResponse> Send()
        {

            StringBuilder smsXML = new StringBuilder(smsTemplate);
            StringBuilder messagesXML = new StringBuilder();
            StringBuilder messageXML = new StringBuilder();
            messagesXML.Append("<SMS>");
            foreach (Message msg in Messages)
            {
                foreach (string recipient in msg.Recipients)
                {
                    messageXML.Append("<kisi>");
                    messageXML.AppendFormat("<mesaj>{0}</mesaj>", msg.Body);
                    messageXML.AppendFormat("<gsm>{0}</gsm>", recipient);
                    messageXML.Append("</kisi>");
                    messagesXML.Append(messageXML);
                    messageXML.Clear();
                }
            }
            messagesXML.Append("</SMS>");

            smsXML.Replace("{userName}", this.userName);
            smsXML.Replace("{password}", this.password);
            smsXML.Replace("{origin}", this.Alias);
            smsXML.Replace("{messages}", messagesXML.ToString());

            return await SendHTTP(smsXML.ToString());
        }

        private async Task<SMSResponse> SendHTTP(string xmlMessages)
        {
            NotificationStatus status;
            string sWebPage = "";
            string sCode = "";

            try
            {
                WebClient wUpload = new WebClient();
                wUpload.Headers.Add("Content-Type", "text/xml");
                wUpload.Proxy = null;
                byte[] bPostArray = Encoding.UTF8.GetBytes(xmlMessages);
                byte[] bResponse = await wUpload.UploadDataTaskAsync(this.apiAddress, "POST", bPostArray);
                char[] sReturnChars = Encoding.UTF8.GetChars(bResponse);
                sWebPage = new string(sReturnChars);

                var match = resultRegex.Match(sWebPage);

                if (match.Success && match.Groups.Count >= 1)
                {
                    sWebPage = match.Groups[1].Value;
                    if (sWebPage.Contains(","))
                    {
                        sCode = sWebPage.Split(',')[0];
                    }
                    else if (sWebPage.Contains("*OK*"))
                    {
                        sCode = "OK";
                    }
                }
                else
                {
                    sCode = "";
                }

                switch (sCode)
                {
                    case "00":
                        status = NotificationStatus.InvalidUserNamePassword;
                        break;
                    case "-1":
                        status = NotificationStatus.InvalidUserNamePassword;
                        break;
                    case "-5":
                        status = NotificationStatus.OtherError;
                        break;
                    case "03":
                        status = NotificationStatus.InsufficentCredits;
                        break;
                    case "13":
                        status = NotificationStatus.OtherError;
                        break;
                    case "05":
                        status = NotificationStatus.OtherError;
                        break;
                    case "15":
                        status = NotificationStatus.OtherError;
                        break;
                    case "OK":
                        status = NotificationStatus.Successful;
                        break;
                    default:
                        status = NotificationStatus.OtherError;
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
