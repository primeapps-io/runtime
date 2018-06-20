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
using PrimeApps.Model.Common.Resources;

namespace PrimeApps.App.Jobs
{
    public class TrialNotification
    {
        [CommonQueue, DisableConcurrentExecution(360)]
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
				
				var culture = "";

				if (!tenant.App.UseTenantSettings)
					culture = tenant.App.Setting.Culture;
				else if (!tenant.UseUserSettings)
					culture = tenant.Setting.Culture;
				else
					culture = user.Setting.Culture;
				
                emailData.Add("UserName", user.FirstName + " " + user.LastName);

                if (!string.IsNullOrWhiteSpace(culture) && Helpers.Constants.CULTURES.Contains(culture))
                    Thread.CurrentThread.CurrentCulture = CultureInfo.CreateSpecificCulture(culture);

                var notification = new Helpers.Email(EmailResource.TrialExpireMail, Thread.CurrentThread.CurrentCulture.Name, emailData, tenant.AppId);
                notification.AddRecipient(user.Email);
                notification.AddToQueue();
            }
        }
    }
}