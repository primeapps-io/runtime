using PrimeApps.App.Jobs.Messaging.SMS;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.App.Jobs.Email
{
    public class Messaging
    {
        /// <summary>
        /// Sends email status notification about bulk email job.
        /// </summary>
        /// <param name="owner"></param>
        /// <param name="template"></param>
        /// <param name="moduleName"></param>
        /// <param name="smsDate"></param>
        /// <param name="status"></param>
        /// <param name="successful"></param>
        /// <param name="notAllowed"></param>
        /// <param name="missingAddresses"></param>
        public static void SendEMailStatusNotification(TenantUser owner, string template, string moduleName, DateTime smsDate, NotificationStatus status, int successful, int notAllowed, int missingAddresses, int tenantId)
        {
            string formattedDate = "";

            using (var dbContext = new TenantDBContext(tenantId))
            {
                CultureInfo cultureInfo = new CultureInfo(owner.Culture);
                formattedDate = smsDate.ToString("dd.MM.yyyy", cultureInfo);
                //create email mesage with its parameters.
                Dictionary<string, string> emailData = new Dictionary<string, string>();
                Helpers.Email email = null;
                emailData.Add("QueueDate", formattedDate);
                emailData.Add("UserName", $"{owner.FirstName} {owner.LastName}");
                emailData.Add("Template", template.Replace("{", "[").Replace("}", "]"));
                emailData.Add("Module", moduleName);

                if (status == NotificationStatus.Successful)
                {
                    emailData.Add("Successful", successful.ToString());
                    emailData.Add("MissingNumbers", missingAddresses.ToString());
                    emailData.Add("NotAllowed", notAllowed.ToString());

                    email = new Helpers.Email(typeof(Resources.Email.EMailStatusSuccessful), owner.Culture, emailData);
                }
                else
                {
                    emailData.Add("ErrorReason", $"{{{status.ToString()}}}");
                    email = new Helpers.Email(typeof(Resources.Email.EMailStatusFailed), owner.Culture, emailData);
                }

                /// send the email.
                email.AddRecipient(owner.Email);
                email.AddToQueue();

                //transaction.Commit();//TODO : Not working before now, Why?..
            }

        }
    }
}
