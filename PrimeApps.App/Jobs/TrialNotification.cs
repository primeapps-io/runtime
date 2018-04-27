using Hangfire;
using PrimeApps.App.Jobs.QueueAttributes;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.App.Jobs
{
    public class TrialNotification
    {
        [CommonQueue, AutomaticRetry(Attempts = 0), DisableConcurrentExecution(360)]
        public async Task TrialExpire()
        {
            IList<Tenant> trialTenants = new List<Tenant>();

            using (var platformDBContext = new PlatformDBContext())
            using (var tenantRepository = new TenantRepository(platformDBContext))
            {
                trialTenants = await tenantRepository.GetTrialTenants();
            }

            foreach (var tenant in trialTenants)
            {
                var emailData = new Dictionary<string, string>();
                var tenantUser = tenant.TenantUsers.Single();
                var user = tenantUser.PlatformUser;
                emailData.Add("UserName", user.FirstName + " " + user.LastName);

                if (!string.IsNullOrWhiteSpace(user.Culture) && Helpers.Constants.CULTURES.Contains(user.Culture))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(user.Culture);

                var notification = new Helpers.Email(typeof(Resources.Email.TrialExpireMail), Thread.CurrentThread.CurrentCulture.Name, emailData, tenant.AppId);
                notification.AddRecipient(user.Email);
                notification.AddToQueue();
            }
        }
    }
}