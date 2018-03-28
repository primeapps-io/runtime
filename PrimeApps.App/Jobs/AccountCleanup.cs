using Elmah;
using Hangfire;
using Npgsql;
using PrimeApps.App.Jobs.QueueAttributes;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs
{
    [CommonQueue, AutomaticRetry(Attempts = 0), DisableConcurrentExecution(360)]
    /// Drops tenant databases that are inactive more then 1 month from servers.
    public class AccountCleanup
    {
        /// <summary>
        /// Execute the job
        /// </summary>
        public async Task Run()
        {
            IList<int> expiredTenants = new List<int>();

            using (PlatformDBContext platformDbContext = new PlatformDBContext())
            using (PlatformUserRepository platformUserRepository = new PlatformUserRepository(platformDbContext))
            {

                // Get expired inactive tenant ids.
                expiredTenants = await platformUserRepository.GetExpiredTenantIdsToDelete();
            }
            var dropSql = $"DROP DATABASE IF EXISTS";

            // create a connection to the server without specifying a database.
            using (var connection = new NpgsqlConnection(Postgres.GetConnectionString(-1)))
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
                        }
                    }
                }

                // close the connection before it is disposed by the current using scope.
                connection.Close();
            }
        }
    }
}