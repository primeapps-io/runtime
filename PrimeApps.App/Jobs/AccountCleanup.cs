using Npgsql;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.App.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http;

namespace PrimeApps.App.Jobs
{
	/// Drops tenant databases that are inactive more then 1 month from servers.
	public class AccountCleanup
	{
		private IConfiguration _configuration;
		private IServiceScopeFactory _serviceScopeFactory;

		public AccountCleanup(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
		{
			_configuration = configuration;
			_serviceScopeFactory = serviceScopeFactory;
		}

		/// <summary>
		/// Execute the job
		/// </summary>
		public async Task Run()
		{
			IList<int> expiredTenants = new List<int>();
			var connectionString = "";

			using (var scope = _serviceScopeFactory.CreateScope())
			{
				var platformDBContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
				using (var tenantRepository = new TenantRepository(platformDBContext, _configuration))
				{
					// Get expired inactive tenant ids.
					expiredTenants = await tenantRepository.GetExpiredTenantIdsToDelete();
					connectionString = platformDBContext.Database.GetDbConnection().ConnectionString;
				}



				foreach (var tenantId in expiredTenants)
				{
					var dropSql = "DROP DATABASE IF EXISTS tenant" + tenantId+";";
					try
					{
						platformDBContext.Database.ExecuteSqlCommand(dropSql);

					}
					catch (Exception ex)
					{
						// the query will fail in any unexpected case rather then non-existing database, and it will be logged here with the details. 
						//Error err = new Error(ex);
						//err.Detail = $"Expired tenant {tenantId} could not be removed by Account Cleanup Job";
						//ErrorLog.GetDefault(null).Log(err);
						ErrorHandler.LogError(ex, $"Expired tenant {tenantId} could not be removed by Account Cleanup Job" + " " + "tenant_id:" + tenantId);
					}
				}
			}
		}
	}
}