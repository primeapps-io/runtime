using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Entities.Tenant;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Role;
using PrimeApps.Model.Helpers;
using Microsoft.AspNetCore.Mvc.Filters;
using Hangfire;

namespace PrimeApps.App.Controllers
{
    [Route("api/role"), Authorize]
	public class RoleController : ApiBaseController
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

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_userRepository);
			SetCurrentUser(_roleRepository);

			base.OnActionExecuting(context);
		}

		[Route("find"), HttpPost]
        public async Task<Role> Find([FromQuery(Name = "id")]int id)
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
        public async Task Create([FromBody]RoleDTO role)
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
        public async Task Update([FromQuery]bool roleChange, [FromBody]RoleDTO role)
        {
            Role roleToUpdate = await _roleRepository.GetByIdAsyncWithUsers(role.Id);
            if (roleToUpdate == null) return;

            await _roleRepository.UpdateAsync(roleToUpdate, role);

            if (roleChange)
                BackgroundJob.Enqueue(() => UpdateUserRoleBulk());
        }

        [Route("delete"), HttpDelete]
        public async Task Delete([FromQuery(Name = "id")]int id, [FromQuery(Name = "transferRoleId")]int transferRoleId)
        {
            await _roleRepository.RemoveAsync(id, transferRoleId);
        }

        [Route("update_user_role"), HttpPut]
        public async Task UpdateUserRole([FromQuery(Name = "userId")]int userId, [FromQuery(Name = "roleId")]int roleId)
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