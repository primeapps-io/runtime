using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Warehouse;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Jobs
{
    public class Warehouse
    {
        private IWarehouseRepository _warehouseRepository;
        private ITenantRepository _tenantRepository;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
         public Warehouse(IWarehouseRepository warehouseRepository, ITenantRepository tenantRepository, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _warehouseRepository = warehouseRepository;
            _tenantRepository = tenantRepository;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }
  
        public async Task Create(WarehouseCreateRequest request, UserItem appUser)
        {
            _warehouseRepository.TenantId = request.TenantId;
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var moduleRepository = new ModuleRepository(databaseContext, _configuration))
                {
                    moduleRepository.UserId = request.TenantId;
                    moduleRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = request.TenantId, UserId = request.TenantId };
                    var modules = await moduleRepository.GetAll();
                    var tenantLanguage = appUser.TenantLanguage;

                    await _warehouseRepository.Create(request, modules, appUser.Email, tenantLanguage);
                }
            }

        }

        public void ChangePassword(WarehousePasswordRequest request, PlatformWarehouse warehouse)
        {
            _warehouseRepository.ChangePassword(warehouse.DatabaseUser, request.DatabasePassword);
        }
    }
}