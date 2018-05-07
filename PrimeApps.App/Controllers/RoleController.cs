using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Entities.Application;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Role;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Controllers
{
    [Route("api/role"), Authorize/*, SnakeCase*/]
	public class RoleController : BaseController
    {
        private IRoleRepository _roleRepository;
        private IUserRepository _userRepository;
        private Warehouse _warehouse;
        public RoleController(IRoleRepository roleRepository, IUserRepository userRespository, Warehouse warehouse)
        {
            _roleRepository = roleRepository;
            _userRepository = userRespository;
            _warehouse = warehouse;
        }

        [Route("find"), HttpPost]
        public async Task<Role> Find([FromRoute]int id)
        {
            return await _roleRepository.GetByIdAsync(id);
        }

        [Route("find_all"), HttpPost]
        public async Task<IEnumerable<RoleDTO>> FindAll()
        {
            IEnumerable<RoleDTO> roles = await _roleRepository.GetAllAsync();
            
            return roles;
        }

        [Route("create"), HttpPost]
        public async Task Create(RoleDTO role)
        {
            await _roleRepository.CreateAsync(new Role()
            {
                LabelEn = role.LabelEn,
                LabelTr = role.LabelTr,
                DescriptionEn = role.DescriptionEn,
                DescriptionTr = role.DescriptionTr,
                Master = role.Master,
                OwnersList = role.Owners,
                ReportsToId = role.ReportsTo,
                ShareData = role.ShareData
            });
        }

        [Route("update"), HttpPut]
        public async Task Update([FromBody]RoleDTO role)
        {
            Role roleToUpdate = await _roleRepository.GetByIdAsyncWithUsers(role.Id);
            if (roleToUpdate == null) return;

            await _roleRepository.UpdateAsync(roleToUpdate, role);
        }

        [Route("delete"), HttpDelete]
        public async Task Delete([FromRoute]int id, [FromRoute]int transferRoleId)
        {
            await _roleRepository.RemoveAsync(id, transferRoleId);
        }

        [Route("update_user_role"), HttpPut]
        public async Task UpdateUserRole([FromRoute]int userId, [FromRoute]int roleId)
        {
            var user = await _userRepository.GetById(userId);

            if (user.RoleId.HasValue)
            {
                await _roleRepository.RemoveUserAsync(user.Id, user.RoleId.Value);
            }

            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            await _roleRepository.AddUserAsync(user.Id, roleId);
        }

        [Route("update_user_role_bulk"), HttpPut]
        public async Task UpdateUserRoleBulk()
        {
            var users = await _userRepository.GetAllAsync();

            foreach (var user in users)
            {
                if (user.RoleId.HasValue)
                {
                    await _roleRepository.RemoveUserAsync(user.Id, user.RoleId.Value);
                }

                _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

                await _roleRepository.AddUserAsync(user.Id, user.RoleId.Value);
            }
        }
    }
}