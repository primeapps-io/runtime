using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.App.Helpers;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Jobs
{
    public class AccountCleanup
    {
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public AccountCleanup(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task Run()
        {
            using (var scope = _serviceScopeFactory.CreateScope())
            {
                IList<int> expiredTenants;
                var platformDBContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var cacheHelper = scope.ServiceProvider.GetRequiredService<ICacheHelper>();


                using (var tenantRepository = new TenantRepository(platformDBContext, _configuration, cacheHelper))
                {
                    expiredTenants = await tenantRepository.GetExpiredTenantIdsToDelete();
                }

                foreach (var tenantId in expiredTenants)
                {
                    var dropSql = "DROP DATABASE IF EXISTS tenant" + tenantId + ";";
                    try
                    {
                        platformDBContext.Database.ExecuteSqlCommand(dropSql);
                    }
                    catch (Exception ex)
                    {
                        ErrorHandler.LogError(ex, $"Expired tenant {tenantId} could not be removed by Account Cleanup Job" + " " + "tenant_id:" + tenantId);
                    }
                }
            }
        }
    }
}