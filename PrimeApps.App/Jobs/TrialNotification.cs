using Hangfire;
using PrimeApps.App.Jobs.QueueAttributes;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform.Identity;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.IdentityModel.Protocols;

namespace PrimeApps.App.Jobs
{
    public class TrialNotification
    {
        [CommonQueue, AutomaticRetry(Attempts = 0), DisableConcurrentExecution(360)]
        public async Task TrialExpire()
        {
            IList<PlatformUser> trialUsers = new List<PlatformUser>();

            using (PlatformDBContext platformDBContext = new PlatformDBContext())
            {
                using (PlatformUserRepository platformUserRepository = new PlatformUserRepository(platformDBContext))
                {
                    trialUsers = await platformUserRepository.GetTrialUsers();
                }
            }

            foreach (var user in trialUsers)
            {
                var emailData = new Dictionary<string, string>();
                string domain;

                domain = "https://{0}.ofisim.com/";
                var appDomain = "crm";

                switch (user.AppId)
                {
                    case 2:
                        appDomain = "kobi";
                        break;
                    case 3:
                        appDomain = "asistan";
                        break;
                    case 4:
                        appDomain = "ik";
                        break;
                    case 5:
                        appDomain = "cagri";
                        break;
                }

                var subdomain = ConfigurationManager.AppSettings.Get("TestMode") == "true" ? "test" : appDomain;
                domain = string.Format(domain, subdomain);

                //domain = "http://localhost:5554/";

                emailData.Add("UserName", user.FirstName + " " + user.LastName);

                if (!string.IsNullOrWhiteSpace(user.Culture) && Helpers.Constants.CULTURES.Contains(user.Culture))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(user.Culture);

                var notification = new Helpers.Email(typeof(Resources.Email.TrialExpireMail), Thread.CurrentThread.CurrentCulture.Name, emailData, user.AppId);
                notification.AddRecipient(user.Email);
                notification.AddToQueue();
            }
        }
    }
}