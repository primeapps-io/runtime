using Hangfire;
using PrimeApps.App.Jobs.QueueAttributes;
using PrimeApps.Model.Context;
using System.Threading.Tasks;
using Npgsql;
using PrimeApps.Model.Helpers.QueryTranslation;
using PrimeApps.Model.Repositories;
using System.Data;

namespace PrimeApps.App.Jobs
{
    public class AccountDeactivate
    {
        [CommonQueue, AutomaticRetry(Attempts = 0), DisableConcurrentExecution(360)]
        public async Task Deactivate()
        {
            using (var platformDbContext = new PlatformDBContext())
            {
                using (var tenantRepository = new TenantRepository(platformDbContext))
                {

                    var tenants = await tenantRepository.GetExpiredTenants();

                    foreach (var tenant in tenants)
                    {
                        using (var databaseContext = new TenantDBContext(tenant.Id))
                        using (var userRepository = new UserRepository(databaseContext))
                        {
                            var users = await userRepository.GetAllAsync();

                            foreach (var user in users)
                            {
                                try
                                {
                                    user.IsActive = false;
                                    databaseContext.SaveChanges();
                                }
                                //TODO: ex.InnerException.InnerException olabilir
                                catch (DataException ex)
                                {
                                    if (ex.InnerException is PostgresException)
                                    {
                                        var innerEx = (PostgresException)ex.InnerException;

                                        if (innerEx.SqlState == PostgreSqlStateCodes.DatabaseDoesNotExist)
                                            continue;
                                    }

                                    throw;
                                }

                                await Cache.User.Remove(user.Id);
                            }
                        }

                        tenant.License.IsDeactivated = true;
                        await tenantRepository.UpdateAsync(tenant);
                    }
                }
            }
        }
    }
}