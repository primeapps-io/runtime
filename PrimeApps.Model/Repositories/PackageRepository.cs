using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories
{
    public class PackageRepository : RepositoryBaseStudio, IPackageRepository
    {
        public PackageRepository(StudioDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<Package> GetLastPackage(int appId)
        {
            return await DbContext.Packages.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.AppId == appId);
        }

        public async Task<int> Count(int appId)
        {
            return await DbContext.Packages
                .Where(x => !x.Deleted & x.AppId == appId)
                .CountAsync();
        }

        public async Task<Package> Get(int id)
        {
            return await DbContext.Packages
                .Where(x => !x.Deleted && x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Package>> GetAll(int appId)
        {
            return await DbContext.Packages
                .Where(x => x.AppId == appId && !x.Deleted).ToListAsync();
        }

        public async Task<Package> GetByVersion(int version)
        {
            return await DbContext.Packages
                .Where(x => !x.Deleted && x.Version == version.ToString())
                .FirstOrDefaultAsync();
        }

        public async Task<Package> GetActiveProcess(int appId)
        {
            return await DbContext.Packages
                .FirstOrDefaultAsync(x => x.AppId == appId && x.Status == ReleaseStatus.Running && !x.Deleted);

            /*if (result != null)
                return result;

            result = await DbContext.Packages.LastOrDefaultAsync(x => x.AppId == appId);

            if (result != null)
            {
                var settings = JObject.Parse(result.Settings);

                if (settings["type"].ToString() == "publish" && !result.Published)
                    return result;
            }

            return null;*/
        }

        public async Task<ICollection<Package>> Find(int appId, PaginationModel paginationModel)
        {
            var releases = DbContext.Packages
                .Include(x => x.AppDraft)
                .Where(x => !x.Deleted & x.AppId == appId)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Package).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

                if (paginationModel.OrderType == "asc")
                {
                    releases = releases.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    releases = releases.OrderByDescending(x => propertyInfo.GetValue(x, null));
                }
            }

            return await releases.ToListAsync();
        }

        public async Task<int> Create(Package package)
        {
            DbContext.Packages.Add(package);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Package package)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(Package package)
        {
            DbContext.Packages.Remove(package);
            return await DbContext.SaveChangesAsync();
        }
    }
}