using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ITenantRepository : IRepositoryBasePlatform
    {
        Tenant Get(int tenantId);
        Task<Tenant> GetAsync(int tenantId);
        Task<Tenant> GetWithSettingsAsync(int tenantId);
        Task<IList<Tenant>> GetAllActive();
        Task<IList<TenantInfo>> GetTenantInfo(int tenantId);
        Task UpdateAsync(Tenant tenant);
		Task<int> CreateAsync(Tenant tenant);
		Task<int> DeleteAsync(Tenant tenant);

		Task<Tenant> GetByCustomDomain(string customDomain);
        Task<Tenant> GetWithOwnerAsync(int tenantId);
        Task<IList<Tenant>> GetExpiredTenants();
        Task<IList<int>> GetExpiredTenantIdsToDelete();
        Task<IList<Tenant>> GetTrialTenants();
    }
}
