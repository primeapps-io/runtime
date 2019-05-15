using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Warehouse;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Tenant;
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
        private IMenuRepository _menuRepository;
        private IServiceScopeFactory _serviceScopeFactory;
         public Warehouse(IWarehouseRepository warehouseRepository, ITenantRepository tenantRepository, IMenuRepository menuRepository, IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _warehouseRepository = warehouseRepository;
            _tenantRepository = tenantRepository;
            _menuRepository = menuRepository;          
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
                    _menuRepository.TenantId = request.TenantId;
                    moduleRepository.UserId = request.TenantId;
                    moduleRepository.CurrentUser = new Model.Helpers.CurrentUser { TenantId = request.TenantId, UserId = request.TenantId };
                    var modules = await moduleRepository.GetAll();
                    var tenantLanguage = appUser.TenantLanguage;

                    await _warehouseRepository.Create(request, modules, appUser.Email, tenantLanguage);
                    var menuItems = await _menuRepository.GetAllMenuItems();

                    if (menuItems != null && menuItems.Count > 0)
                    {
                        foreach (var menuItem in menuItems)
                        {
                            if (menuItem.Route == "analytics")
                                continue;
                           
                            var menu = await _menuRepository.GetByProfileId((int)appUser.ProfileId);

                            MenuItem menuItemAdd = new MenuItem()
                            {
                                MenuId = menu.Id,
                                ModuleId = null,
                                ParentId = null,
                                Route = "analytics",
                                LabelEn = "İş Zekası",
                                LabelTr = "İş Zekası",
                                MenuIcon = "fa fa-line-chart",
                                Order = (short)(menuItems.Count + 1),
                                IsDynamic = false,
                                Deleted = false
                            };

                            await _menuRepository.CreateMenuItems(menuItemAdd);
                        }
                    }
                }
            }

        }

        public void ChangePassword(WarehousePasswordRequest request, PlatformWarehouse warehouse)
        {
            _warehouseRepository.ChangePassword(warehouse.DatabaseUser, request.DatabasePassword);
        }
    }
}