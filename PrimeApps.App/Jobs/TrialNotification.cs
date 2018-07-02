using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Resources;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs
{
    public class TrialNotification
    {
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public TrialNotification(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task TrialExpire()
        {
            IList<Tenant> trialTenants = new List<Tenant>();

            using (var platformDBContext = new PlatformDBContext(_configuration))
            using (var tenantRepository = new TenantRepository(platformDBContext, _configuration))
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

                var appUser = new UserItem { AppId = tenant.AppId, Id = tenant.OwnerId };
                var notification = new Helpers.Email(EmailResource.TrialExpireMail, Thread.CurrentThread.CurrentCulture.Name, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
                notification.AddRecipient(user.Email);
                notification.AddToQueue();
            }
        }
    }
}