using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Resources;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
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
			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var platformDBContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();

				using (var tenantRepository = new TenantRepository(platformDBContext, _configuration, cacheHelper))
				{
					var tenants = await tenantRepository.GetAllActive();

					foreach (var tenant in tenants)
					{
						if (!string.IsNullOrEmpty(previewMode))
						{
							tenantRepository.CurrentUser = new CurrentUser { TenantId = tenant.Id, UserId = tenant.OwnerId, PreviewMode = previewMode };
						}
						trialTenants = await tenantRepository.GetTrialTenants();

						if (trialTenants.Count == 0)
							continue;
					}
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

					var appUser = new UserItem { AppId = tenant.AppId, Id = tenant.OwnerId, TenantId = tenant.Id };
					var notification = new Helpers.Email(EmailResource.TrialExpireMail, Thread.CurrentThread.CurrentCulture.Name, emailData, _configuration, _serviceScopeFactory, appUser.AppId, appUser);
					notification.AddRecipient(user.Email);
					notification.AddToQueue();
				}
			}
		}
	}
}