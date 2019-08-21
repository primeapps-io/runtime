using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Repositories
{
    public class ReleaseRepository : RepositoryBasePlatform, IReleaseRepository
    {
        public ReleaseRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }
        
        public async Task<int> Count(int appId)
        {
            return await DbContext.Releases
                .Where(x => !x.Deleted & x.AppId == appId)
                .CountAsync();
        }

        public async Task<Release> Get(int id)
        {
            return await DbContext.Releases
                .Where(x => !x.Deleted && x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Release> GetByVersion(int version)
        {
            return await DbContext.Releases
                .Where(x => !x.Deleted && x.Version == version.ToString())
                .FirstOrDefaultAsync();
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