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

namespace PrimeApps.Model.Repositories
{
    public class DeploymentFunctionRepository : RepositoryBaseTenant, IDeploymentFunctionRepository
    {
        public DeploymentFunctionRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<int> Count(int functionId)
        {
            return await DbContext.DeploymentsFunction
               .Where(x => !x.Deleted & x.FunctionId == functionId)
               .CountAsync();
        }
        public bool AvailableForDeployment(int functionId)
        {
            return DbContext.DeploymentsFunction
                       .Count(x => x.FunctionId == functionId && x.Status == DeploymentStatus.Running && !x.Deleted) == 0;
        }
       
        public async Task<DeploymentFunction> Get(int id)
        {
            return await DbContext.DeploymentsFunction
               .Where(x => !x.Deleted && x.Id == id)
               .FirstOrDefaultAsync();
        }

        public async Task<int> CurrentBuildNumber(int functionId)
        {
            return await DbContext.DeploymentsFunction
                .OrderByDescending(x => x.Id)
                .Where(x => x.FunctionId == functionId)
                .Select(x => x.BuildNumber)
                .FirstOrDefaultAsync();
        }

        public async Task<ICollection<DeploymentFunction>> Find(int functionId, PaginationModel paginationModel)
        {
            var deployments = await DbContext.DeploymentsFunction
                .Include(x => x.Function)
                .Where(x => !x.Deleted & x.FunctionId == functionId)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit)
                .ToListAsync();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Module).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    deployments = deployments.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    deployments = deployments.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }

            }

            return deployments;
        }

        public async Task<int> Create(DeploymentFunction deployment)
        {
            DbContext.DeploymentsFunction.Add(deployment);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(DeploymentFunction deployment)
        {
            return await DbContext.SaveChangesAsync();
        }
        public async Task<int> Delete(DeploymentFunction deployment)
        {
            DbContext.DeploymentsFunction.Remove(deployment);
            return await DbContext.SaveChangesAsync();
        }
    }
}
