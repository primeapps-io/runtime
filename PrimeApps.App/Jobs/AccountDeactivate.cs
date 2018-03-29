using Hangfire;
using PrimeApps.App.Jobs.QueueAttributes;
using PrimeApps.Model.Context;
using System.Threading.Tasks;
using Npgsql;
using PrimeApps.Model.Helpers.QueryTranslation;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Entities.Platform.Identity;
using System.Collections.Generic;

namespace PrimeApps.App.Jobs
{
    public class AccountDeactivate
    {
        [CommonQueue, AutomaticRetry(Attempts = 0), DisableConcurrentExecution(360)]
        public async Task Deactivate()
        {
            using (PlatformDBContext platformDbContext = new PlatformDBContext())
            {
                using (TenantRepository tenantRepository = new TenantRepository(platformDbContext))

                using (PlatformUserRepository platformUserRepository = new PlatformUserRepository(platformDbContext))
                {

                    IList<PlatformUser> subscribers = await platformUserRepository.GetExpiredUsers();

                    foreach (var subscriber in subscribers)
                    {
                        using (var databaseContext = new TenantDBContext(subscriber.TenantId.Value))
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
                                catch (EntityException ex)
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
                        var tenant = await tenantRepository.GetAsync(subscriber.TenantId.Value);
                        tenant.IsDeactivated = true;
                        await tenantRepository.UpdateAsync(tenant);
                    }
                }
            }
        }
    }
}