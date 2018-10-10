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

namespace PrimeApps.App.Jobs
{
	/// Drops tenant databases that are inactive more then 1 month from servers.
	public class AccountCleanup
	{
		private IConfiguration _configuration;

		public AccountCleanup(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		/// <summary>
		/// Execute the job
		/// </summary>
		public async Task Run()
		{
			IList<int> expiredTenants = new List<int>();
			var connectionString = "";

			using (var platformDbContext = new PlatformDBContext(_configuration))
			using (var tenantRepository = new TenantRepository(platformDbContext, _configuration))
			{

				// Get expired inactive tenant ids.
				expiredTenants = await tenantRepository.GetExpiredTenantIdsToDelete();
				connectionString = platformDbContext.Database.GetDbConnection().ConnectionString;
			}

			var dropSql = $"DROP DATABASE IF EXISTS";

			// create a connection to the server without specifying a database.
			using (var connection = new NpgsqlConnection(Postgres.GetConnectionString(connectionString, -1)))
			{
				connection.Open();

				foreach (var tenantId in expiredTenants)
				{
					// create a command by using the template in dropSql variable for drop script. Ex. DROP DATABASE IF EXISTS tenant9999; 
					using (var command = new NpgsqlCommand($"{dropSql} tenant{tenantId};", connection))
					{
						try
						{
							// it will execute drop query and won't fail in case database does not exist.
							command.ExecuteNonQuery();
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

				// close the connection before it is disposed by the current using scope.
				connection.Close();
			}
		}
	}
}