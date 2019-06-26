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
    public class ReleaseRepository : RepositoryBaseStudio, IReleaseRepository
    {
        public ReleaseRepository(StudioDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<Release> GetLastRelease(int appId)
        {
            return await DbContext.Releases.OrderByDescending(x => x.Id).FirstOrDefaultAsync(x => x.AppId == appId);
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

        public async Task<int> CurrentBuildNumber(int appId)
        {
            return await DbContext.Releases
                .Where(x => x.AppId == appId)
                .CountAsync();
        }

        public async Task<Release> GetActiveProcess(int appId)
        {
            var result = await DbContext.Releases
                .FirstOrDefaultAsync(x => x.AppId == appId && x.Status == ReleaseStatus.Running && !x.Deleted);

            if (result != null)
                return result;

            result = await DbContext.Releases.LastOrDefaultAsync();

            var settings = JObject.Parse(result.Settings);

            if (settings["type"].ToString() == "publish" && !result.Published)
                return result;

            return null;
        }

        public async Task<ICollection<Release>> Find(int appId, PaginationModel paginationModel)
        {
            var deployments = DbContext.Releases
                .Include(x => x.AppDraft)
                .Where(x => !x.Deleted & x.AppId == appId)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Release).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

                if (paginationModel.OrderType == "asc")
                {
                    deployments = deployments.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    deployments = deployments.OrderByDescending(x => propertyInfo.GetValue(x, null));
                }
            }

            return await deployments.ToListAsync();
        }

        public async Task<int> Create(Release release)
        {
            DbContext.Releases.Add(release);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Release release)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(Release release)
        {
            DbContext.Releases.Remove(release);
            return await DbContext.SaveChangesAsync();
        }
    }
}