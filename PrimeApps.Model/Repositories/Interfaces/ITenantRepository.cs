using PrimeApps.Model.Entities.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Instance;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ITenantRepository : IRepositoryBasePlatform
    {
        Task<Entities.Platform.Tenant> GetAsync(int tenantId);
        Task<IList<TenantInfo>> GetTenantInfo(int tenantId);
        Task UpdateAsync(Entities.Platform.Tenant tenant);
        Task<Entities.Platform.Tenant> GetByCustomDomain(string customDomain);
        Task<Entities.Platform.Tenant> GetWithOwnerAsync(int tenantId);
        Task<int> GetUserCount(int tenantId);

    }
}
