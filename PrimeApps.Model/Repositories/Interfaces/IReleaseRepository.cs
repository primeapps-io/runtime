using System.Threading.Tasks;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IReleaseRepository : IRepositoryBasePlatform
    {
        Task<Release> Get(int id);
        Task<Release> Get(int appId, string version, int? tenantId = null);
        Task<bool> FirstTime(int appId);
        Task<Release> GetLast();
        Task<Release> GetLast(int appId);
        Task<bool> IsThereRunningProcess(int appId);
        Task<int> Create(Release package);
        Task<int> Update(Release package);
        Task<int> Delete(Release package);
    }
}