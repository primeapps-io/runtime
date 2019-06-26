using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IReleaseRepository : IRepositoryBaseStudio
    {
        Task<Release> GetLastRelease(int appId);
        Task<int> Count(int appId);
        Task<Release> Get(int id);
        Task<Release> GetActiveProcess(int appId);
        Task<int> CurrentBuildNumber(int appId);
        Task<ICollection<Release>> Find(int appId, PaginationModel paginationModel);
        Task<int> Create(Release release);
        Task<int> Update(Release release);
        Task<int> Delete(Release release);
    }
}