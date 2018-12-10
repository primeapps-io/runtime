using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public interface IRoleHelper
    {
        Task<bool> UpdateUserRoleBulkAsync(Warehouse warehouse, UserItem appUser);
    }

    public class RoleHelper : IRoleHelper
    {

        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IConfiguration _configuration;

        public RoleHelper(IHttpContextAccessor context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<bool> UpdateUserRoleBulkAsync(Warehouse warehouse, UserItem appUser)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetService<TenantDBContext>();
                warehouse.DatabaseName = appUser.WarehouseDatabaseName;
                using (var userRespository = new UserRepository(databaseContext, _configuration))
                using (var roleRepository = new RoleRepository(databaseContext, warehouse, _configuration))
                {
                    userRespository.CurrentUser = roleRepository.CurrentUser = new CurrentUser { TenantId = appUser.TenantId, UserId = appUser.Id };

                    var users = await userRespository.GetAllAsync();

                    foreach (var user in users)
                    {
                        if (user.RoleId.HasValue)
                        {
                            await roleRepository.RemoveUserAsync(user.Id, user.RoleId.Value);
                        }

                        warehouse.DatabaseName = appUser.WarehouseDatabaseName;

                        await roleRepository.AddUserAsync(user.Id, user.RoleId.Value);

                        //TODO CACHE
                        //await Cache.Workgroup.UpdateRoles(appUser.InstanceId);
                    }


                    return true;
                }
            }
        }
    }
}
