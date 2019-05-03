using IdentityServer4.EntityFramework.DbContexts;

namespace PrimeApps.Auth.Repositories
{
    public class BaseRepository
    {
        public BaseRepository(ConfigurationDbContext dbContext)
        {
            DbContext = dbContext;
        }

        public ConfigurationDbContext DbContext { get; }
    }
}