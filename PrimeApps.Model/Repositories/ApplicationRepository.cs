using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using System.Threading.Tasks;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;

namespace PrimeApps.Model.Repositories
{
    public class ApplicationRepository : RepositoryBasePlatform, IApplicationRepository
    {
        public ApplicationRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }
        public async Task<App> Get(string domain)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Setting.AppDomain == domain);
        }
        public async Task<App> Get(int? id)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<App> GetByNameAsync(string name)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Name == name);
        }

        public App GetByName(string name)
        {
            return DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefault(x => x.Name == name);
        }

        public async Task<App> GetWithAuth(string domain)
        {
            var app = await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Setting.AuthDomain == domain);

            return app;
        }

        public async Task<int> GetAppIdWithDomain(string domain)
        {
            var app = await DbContext.AppSettings
                .FirstOrDefaultAsync(x => x.AppDomain == domain);

            return app?.AppId ?? 0;
        }

        public async Task<App> GetAppWithDomain(string domain)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Setting.AppDomain == domain);
        }
    }
}
