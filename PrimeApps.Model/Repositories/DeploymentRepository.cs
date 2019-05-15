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
    public class DeploymentRepository : RepositoryBaseStudio, IDeploymentRepository
    {
        public DeploymentRepository(StudioDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<int> Count(int appId)
        {
            return await DbContext.Deployments
                .Where(x => !x.Deleted & x.AppId == appId)
                .CountAsync();
        }

        public async Task<Deployment> Get(int id)
        {
            return await DbContext.Deployments
                .Where(x => !x.Deleted && x.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<int> CurrentBuildNumber(int appId)
        {
            return await DbContext.Deployments
                .Where(x => x.AppId == appId)
                .CountAsync();
        }

        public bool AvailableForDeployment(int appId)
        {
            return DbContext.Deployments
                       .Count(x => x.AppId == appId && x.Status == DeploymentStatus.Running && !x.Deleted) == 0;
        }

        public async Task<ICollection<Deployment>> Find(int appId, PaginationModel paginationModel)
        {
            var deployments = DbContext.Deployments
                .Include(x => x.AppDraft)
                .Where(x => !x.Deleted & x.AppId == appId)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Module).GetProperty(paginationModel.OrderColumn);

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

        public async Task<int> Create(Deployment deployment)
        {
            DbContext.Deployments.Add(deployment);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Deployment deployment)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(Deployment deployment)
        {
            DbContext.Deployments.Remove(deployment);
            return await DbContext.SaveChangesAsync();
        }
    }
}