using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IDeploymentComponentRepository : IRepositoryBaseTenant
    {
        Task<int> Count(int componentId);
        Task<DeploymentComponent> Get(int id);
        bool AvailableForDeployment(int componentId);
        Task<int> CurrentBuildNumber(int componentId);
        IQueryable<DeploymentComponent> Find(int componentId);
        Task<int> Create(DeploymentComponent deployment);
        Task<int> Update(DeploymentComponent deployment);
        Task<int> Delete(DeploymentComponent deployment);
    }
}