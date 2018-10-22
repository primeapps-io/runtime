using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Warehouse;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IWarehouseRepository : IRepositoryBaseTenant
    {
        Task Create(WarehouseCreateRequest request, ICollection<Module> modules, string userEmail, string tenantLanguage);
        Task Sync(PlatformWarehouse warehouse, ICollection<Module> modules, string userEmail, string tenantLanguage);
        void ChangePassword(string username, string password);
    }
}
