using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IDeploymentFunctionRepository : IRepositoryBaseTenant
    {
        Task<int> Count(int functionId);
        bool AvailableForDeployment(int functionId);
        Task<DeploymentFunction> Get(int id);
        Task<int> CurrentBuildNumber(int functionId);
        IQueryable<DeploymentFunction> Find(int functionId);
        Task<int> Create(DeploymentFunction deployment);
        Task<int> Update(DeploymentFunction deployment);
        Task<int> Delete(DeploymentFunction deployment);
    }
}