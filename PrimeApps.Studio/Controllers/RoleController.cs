using System.Collections.Generic;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Role;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Controllers
{
	[Route("api/role"), Authorize]
	public class RoleController : DraftBaseController
	{
		private IRoleRepository _roleRepository;
		private IUserRepository _userRepository;
		private IRoleHelper _roleHelper;
		private Warehouse _warehouse;

		public RoleController(IRoleRepository roleRepository, IUserRepository userRespository, IRoleHelper roleHelper, Warehouse warehouse)
		{
			_roleRepository = roleRepository;
			_userRepository = userRespository;
			_roleHelper = roleHelper;
			_warehouse = warehouse;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
			SetCurrentUser(_roleRepository, PreviewMode, TenantId, AppId);

			base.OnActionExecuting(context);
		}

		[Route("find"), HttpPost]
		public async Task<Role> Find([FromQuery(Name = "id")]int id)
		{
			return await _roleRepository.GetByIdAsync(id);
		}

		[Route("get_all"), HttpPost]
		public async Task<IEnumerable<RoleDTO>> GetAll()
		{
			IEnumerable<RoleDTO> roles = await _roleRepository.GetAllAsync();

			return roles;
		}

		[Route("create"), HttpPost]
		public async Task Create([FromBody]RoleDTO role)
		{
			//Set Warehouse
			_warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

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
			}, AppUser.TenantLanguage);
		}

		[Route("update"), HttpPut]
		public async Task Update([FromQuery]bool roleChange, [FromBody]RoleDTO role)
		{
			//var user = await _userRepository.GetById(AppUser.Id);

			// if (!user.Profile.HasAdminRights)
			// return;

			if (role == null)
				return;

			Role roleToUpdate = await _roleRepository.GetByIdAsyncWithUsers(role.Id);
			if (roleToUpdate == null) return;

			await _roleRepository.UpdateAsync(roleToUpdate, role, AppUser.TenantLanguage);

			//Set warehouse database name
			//_warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

			//if (roleChange)
			//	BackgroundJob.Enqueue(() => _roleHelper.UpdateUserRoleBulkAsync(_warehouse, AppUser));
		}

		[Route("delete"), HttpDelete]
		public async Task Delete([FromQuery(Name = "id")]int id, [FromQuery(Name = "transferRoleId")]int transferRoleId)
		{
			//var user = await _userRepository.GetById(AppUser.Id);

			//if (!user.Profile.HasAdminRights)
			//	return;

			if (id <= 0)
				return;

			if (transferRoleId <= 0)
				return;

			await _roleRepository.RemoveAsync(id, transferRoleId);
		}

		[Route("update_user_role"), HttpPut]
		public async Task UpdateUserRole([FromQuery(Name = "user_Id")]int userId, [FromQuery(Name = "role_Id")]int roleId)
		{
			var user = _userRepository.GetById(AppUser.Id);

			if (!user.Profile.HasAdminRights)
				return;

			if (user.RoleId.HasValue)
			{
				await _roleRepository.RemoveUserAsync(userId, user.RoleId.Value);
			}

			_warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

			await _roleRepository.AddUserAsync(userId, roleId);
		}

        [Route("update_user_role_bulk"), HttpPut]
        public void UpdateUserRoleBulk()
        {
            var user = _userRepository.GetById(AppUser.Id);

            if (!user.Profile.HasAdminRights)
                return;

            BackgroundJob.Enqueue(() => _roleHelper.UpdateUserRoleBulkAsync(_warehouse, AppUser));
        }

        [Route("add_owners"), HttpPost]
		public async Task AddOwners([FromQuery(Name = "id")]int id, [FromBody]JArray owners)
		{
			var user = _userRepository.GetById(AppUser.Id);

			if (!user.Profile.HasAdminRights)
				return;

			Role role = await _roleRepository.GetByIdAsyncWithUsers(id);
			await _roleRepository.AddOwners(role, owners);
		}

		[Route("remove_owners"), HttpPost]
		public async Task RemoveOwners([FromQuery(Name = "id")]int id, [FromBody]JArray owners)
		{
			var user = _userRepository.GetById(AppUser.Id);

			if (!user.Profile.HasAdminRights)
				return;

			Role role = await _roleRepository.GetByIdAsyncWithUsers(id);
			await _roleRepository.RemoveOwners(role, owners);
		}
	}
}