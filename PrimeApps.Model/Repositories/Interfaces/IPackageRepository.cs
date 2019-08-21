using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPackageRepository : IRepositoryBaseStudio
    {
        Task<Package> GetLastPackage(int appId);
        Task<int> Count(int appId);
        Task<Package> Get(int id);
        Task<List<Package>> GetAll(int appId);
        Task<Package> GetByVersion(int version);
        Task<Package> GetActiveProcess(int appId);
        Task<ICollection<Package>> Find(int appId, PaginationModel paginationModel);
        Task<int> Create(Package package);
        Task<int> Update(Package package);
        Task<int> Delete(Package package);
    }
}