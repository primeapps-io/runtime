using OfisimCRM.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;
using PrimeApps.App.Jobs.Messaging.SMS;
using PrimeApps.App.Jobs.Messaging.SMS.Providers;
using PrimeApps.Model.Enums;

namespace OfisimCRM.App.Jobs.Messaging.SMS.Providers
{

    /// <summary>
    /// SMS Provider implementation for AjansSMS.
    /// </summary>
    class Netgsm : SMSProvider
    {
        public Netgsm(string userName = "", string password = "") : base(userName, password)
        {
            this.apiAddress = new Uri("http://api.netgsm.com.tr/xmlbulkhttppost.asp");
            this.AcceptedPhoneNumberFormat = new Regex(@"(5)[0-9][0-9][0-9]([0-9]){6}$");
        }

        const string smsTemplate = @"<?xml version='1.0' encoding='UTF-8'?>
                    <mainbody>
                        <header>
                            <company>NETGSM</company>
                            <usercode>{userName}</usercode>
                            <password>{password}</password>                             
                            <type>1:n</type>
                            <startdate></startdate>
		                    <stopdate></stopdate>
                            <msgheader>{originator}</msgheader>
                         </header> 
                            {messages}                                             
                        </mainbody>";

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
                messageXML.Append("<body>");
                messageXML.AppendFormat("<msg> <![CDATA[{0}]]> </msg>", msg.Body);
                foreach (string recipient in msg.Recipients)
                {
                    messageXML.AppendFormat("<no>90{0}</no>", recipient);
                }

                messageXML.Append("</body>");

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
                HttpWebRequest request = WebRequest.Create(this.apiAddress) as HttpWebRequest;
                request.Method = "POST";
                request.ContentType = "text/xml";
                Byte[] bPostArray = Encoding.UTF8.GetBytes(xmlMessages);
                Byte[] bResponse = wUpload.UploadData(this.apiAddress, "POST", bPostArray);
                Char[] sReturnChars = Encoding.UTF8.GetChars(bResponse);
                sWebPage = new string(sReturnChars);

                switch (sWebPage)
                {
                    case "30":
                        status = NotificationStatus.InvalidUserNamePassword;
                        break;
                    case "20":
                        status = NotificationStatus.OtherError;
                        break;
                    case "40":
                        status = NotificationStatus.InvalidAlias;
                        break;
                    case "70":
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
