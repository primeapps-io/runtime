using System.Threading.Tasks;
using Hangfire;
using PrimeApps.Model.Common.Warehouse;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Jobs
{
    public class Warehouse
    {
        private IWarehouseRepository _warehouseRepository;
        private IModuleRepository _moduleRepository;
        private ITenantRepository _tenantRepository;

        public Warehouse(IWarehouseRepository warehouseRepository, IModuleRepository moduleRepository, ITenantRepository tenantRepository)
        {
            _warehouseRepository = warehouseRepository;
            _moduleRepository = moduleRepository;
            _tenantRepository = tenantRepository;
        }

        [QueueAttributes.WarehouseQueue, AutomaticRetry(Attempts = 0)]
        public async Task Create(WarehouseCreateRequest request, string userEmail)
        {
            _moduleRepository.TenantId = request.TenantId;
            _warehouseRepository.TenantId = request.TenantId;

            var modules = await _moduleRepository.GetAll();
            var tenantLanguage = (await _tenantRepository.GetAsync(request.TenantId)).Language;

            var warehouse = new PlatformWarehouse
            {
                DatabaseName = request.DatabaseName,
                DatabaseUser = request.DatabaseUser,
                PowerbiWorkspaceId = request.PowerBiWorkspaceId
            };

            await _warehouseRepository.Create(warehouse, modules, userEmail, tenantLanguage);
        }

        public void ChangePassword(WarehousePasswordRequest request, PlatformWarehouse warehouse)
        {
            _warehouseRepository.ChangePassword(warehouse.DatabaseUser, request.DatabasePassword);
        }
    }
}