using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Resources;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;
using PrimeApps.Model.Common.Cache;

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
        public static void SendEMailStatusNotification(TenantUser owner, string template,string senderAlias,string senderFrom, string moduleName, DateTime smsDate, NotificationStatus status, int successful, int notAllowed, int missingAddresses, int tenantId, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, UserItem appUser)
        {
            string formattedDate = "";

            using (var scope = serviceScopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                dbContext.TenantId = tenantId;

                formattedDate = smsDate.ToString("dd.MM.yyyy");
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

                    email = new Helpers.Email(EmailResource.EMailStatusSuccessful, owner.Culture, emailData, configuration, serviceScopeFactory, appUser.AppId, appUser);
                }
                else
                {
                    emailData.Add("ErrorReason", $"{{{status.ToString()}}}");
                    email = new Helpers.Email(EmailResource.EMailStatusFailed, owner.Culture, emailData, configuration, serviceScopeFactory, appUser.AppId, appUser);
                }

                /// send the email.
                email.AddRecipient(owner.Email);
                email.AddToQueue(from:senderFrom,fromName:senderAlias, appUser: appUser);
            }
        }
    }
}
