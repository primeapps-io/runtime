using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Platform;
using System;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories
{
    public class ReleaseRepository : RepositoryBasePlatform, IReleaseRepository
    {
        public ReleaseRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<Release> Get(int id)
        {
            return await DbContext.Releases
                .Where(x => !x.Deleted && x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Release> Get(int appId, string version, int? tenantId = null)
        {
            return await DbContext.Releases
                .Where(x => !x.Deleted && x.AppId == appId && x.Version == version && x.TenantId == tenantId)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> FirstTime(int appId)
        {
            return (await DbContext.Releases
                        .Where(x => !x.Deleted && x.AppId == appId && x.Status == ReleaseStatus.Succeed && x.TenantId == null)
                        .FirstOrDefaultAsync() == null);
        }

        public async Task<Release> GetLast()
        {
            return await DbContext.Releases
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<Release> GetLast(int appId)
        {
            return await DbContext.Releases
                .Where(x => !x.Deleted && x.AppId == appId && x.TenantId == null)
                .OrderByDescending(x => x.Id)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> IsThereRunningProcess(int appId)
        {
            var result = await DbContext.Releases
                .Where(x => !x.Deleted && x.AppId == appId && x.Status == Enums.ReleaseStatus.Running)
                .AnyAsync();

            return result;
        }

        public async Task<int> Create(Release package)
        {
            DbContext.Releases.Add(package);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Release package)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(Release package)
        {
            DbContext.Releases.Remove(package);
            return await DbContext.SaveChangesAsync();
        }
    }
}