using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ITenantRepository : IRepositoryBasePlatform
    {
        Task<Tenant> GetAsync(int tenantId);
        Task<IList<TenantInfo>> GetTenantInfo(int tenantId);
        Task UpdateAsync(Tenant tenant);
        Task<Tenant> GetByCustomDomain(string customDomain);
        Task<Tenant> GetWithOwnerAsync(int tenantId);
        Task<IList<Tenant>> GetExpiredTenants();
    }
}
