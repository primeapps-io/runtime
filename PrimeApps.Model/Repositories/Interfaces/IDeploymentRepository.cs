using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IDeploymentRepository : IRepositoryBaseStudio
    {
        Task<int> Count(int appId);
        Task<Deployment> Get(int id);
        bool AvailableForDeployment(int appId);
        Task<int> CurrentBuildNumber(int appId);
        Task<ICollection<Deployment>> Find(int appId, PaginationModel paginationModel);
        Task<int> Create(Deployment deployment);
        Task<int> Update(Deployment deployment);
        Task<int> Delete(Deployment deployment);
    }
}