using System.Threading.Tasks;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IReleaseRepository : IRepositoryBasePlatform
    {
        Task<int> Count(int appId);
        Task<Release> Get(int id);
        Task<Release> Get(int appId, string version);
        Task<Release> GetByAppId(int appId);
        Task<bool> IsThereRunningProcess(int appId);
        Task<Release> GetByVersion(int version);
        Task<int> Create(Release package);
        Task<int> Update(Release package);
        Task<int> Delete(Release package);
    }
}