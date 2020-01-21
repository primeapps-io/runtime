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
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Services;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Controllers
{
    [Route("api/role"), Authorize]
    public class RoleController : ApiBaseController
    {
        private IRoleRepository _roleRepository;
        private IUserRepository _userRepository;
        private IRoleHelper _roleHelper;
        private ISettingRepository _settingRepository;
        private Warehouse _warehouse;

        public RoleController(IRoleRepository roleRepository, IUserRepository userRespository, IRoleHelper roleHelper, ISettingRepository settingRepository, Warehouse warehouse)
        {
            _roleRepository = roleRepository;
            _userRepository = userRespository;
            _roleHelper = roleHelper;
            _settingRepository = settingRepository;
            _warehouse = warehouse;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_userRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_roleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_settingRepository, PreviewMode, TenantId, AppId);

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
                ShareData = role.ShareData,
                SystemType = SystemType.Custom
            }, AppUser.TenantLanguage);
        }

        [Route("update"), HttpPut]
        public async Task Update([FromQuery]bool roleChange, [FromBody]RoleDTO role)
        {
            var user = _userRepository.GetById(AppUser.Id);

            var customProfileSetting = await _settingRepository.GetByKeyAsync("custom_profile_permissions");
            var customRolePermission = false;

            if (customProfileSetting != null)
            {
                JToken profileSetting = JObject.Parse(customProfileSetting.Value)["profilePermissions"].Where(x => (int)x["profileId"] == AppUser.ProfileId).FirstOrDefault();
                if (!profileSetting.IsNullOrEmpty())
                {
                    var hasBulkUpdatePermision = profileSetting["permissions"].Any(x => x.Value<string>() == "users");
                    customRolePermission = hasBulkUpdatePermision;
                }
            }

            if (!user.Profile.HasAdminRights && !customRolePermission)
                return;

            Role roleToUpdate = await _roleRepository.GetByIdAsyncWithUsers(role.Id);
            if (roleToUpdate == null || roleToUpdate.SystemType == SystemType.System) return;

            role.SystemType = SystemType.Custom;
            await _roleRepository.UpdateAsync(roleToUpdate, role, AppUser.TenantLanguage);

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            if (roleChange)
                BackgroundJob.Enqueue(() => _roleHelper.UpdateUserRoleBulkAsync(_warehouse, AppUser));
        }

        [Route("delete"), HttpDelete]
        public async Task Delete([FromQuery(Name = "id")]int id, [FromQuery(Name = "transferRoleId")]int transferRoleId)
        {
            var role = await _roleRepository.GetByIdAsync(id);

            if (role == null || role.SystemType == SystemType.System)
                return;

            var user = _userRepository.GetById(AppUser.Id);
            var customProfileSetting = await _settingRepository.GetByKeyAsync("custom_profile_permissions");
            var customRolePermission = false;

            if (customProfileSetting != null)
            {
                JToken profileSetting = JObject.Parse(customProfileSetting.Value)["profilePermissions"].Where(x => (int)x["profileId"] == AppUser.ProfileId).FirstOrDefault();
                if (!profileSetting.IsNullOrEmpty())
                {
                    var hasBulkUpdatePermision = profileSetting["permissions"].Any(x => x.Value<string>() == "users");
                    customRolePermission = hasBulkUpdatePermision;
                }
            }

            if (!user.Profile.HasAdminRights && !customRolePermission)
                return;

            await _roleRepository.RemoveAsync(id, transferRoleId);
        }

        [Route("update_user_role"), HttpPut]
        public async Task UpdateUserRole([FromQuery(Name = "user_Id")]int userId, [FromQuery(Name = "role_Id")]int roleId)
        {
            var user = _userRepository.GetById(AppUser.Id);

            var customProfileSetting = await _settingRepository.GetByKeyAsync("custom_profile_permissions");
            var customRolePermission = false;

            if (customProfileSetting != null)
            {
                JToken profileSetting = JObject.Parse(customProfileSetting.Value)["profilePermissions"].Where(x => (int)x["profileId"] == AppUser.ProfileId).FirstOrDefault();
                if (!profileSetting.IsNullOrEmpty())
                {
                    var hasBulkUpdatePermision = profileSetting["permissions"].Any(x => x.Value<string>() == "users");
                    customRolePermission = hasBulkUpdatePermision;
                }
            }

            if (!user.Profile.HasAdminRights && !customRolePermission)
                return;

            if (user.RoleId.HasValue)
            {
                await _roleRepository.RemoveUserAsync(userId, user.RoleId.Value);
            }

            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            await _roleRepository.AddUserAsync(userId, roleId);
        }

        [Route("update_user_role_bulk"), HttpPut]
        public async void UpdateUserRoleBulk()
        {
            var user = _userRepository.GetById(AppUser.Id);

            var customProfileSetting = await _settingRepository.GetByKeyAsync("custom_profile_permissions");
            var customRolePermission = false;

            if (customProfileSetting != null)
            {
                JToken profileSetting = JObject.Parse(customProfileSetting.Value)["profilePermissions"].Where(x => (int)x["profileId"] == AppUser.ProfileId).FirstOrDefault();
                if (!profileSetting.IsNullOrEmpty())
                {
                    var hasBulkUpdatePermision = profileSetting["permissions"].Any(x => x.Value<string>() == "users");
                    customRolePermission = hasBulkUpdatePermision;
                }
            }

            if (!user.Profile.HasAdminRights && !customRolePermission)
                return;

            BackgroundJob.Enqueue(() => _roleHelper.UpdateUserRoleBulkAsync(_warehouse, AppUser));
        }

        [Route("add_owners"), HttpPost]
        public async Task AddOwners([FromQuery(Name = "id")]int id, [FromBody]JArray owners)
        {
            var user = _userRepository.GetById(AppUser.Id);

            var customProfileSetting = await _settingRepository.GetByKeyAsync("custom_profile_permissions");
            var customRolePermission = false;

            if (customProfileSetting != null)
            {
                JToken profileSetting = JObject.Parse(customProfileSetting.Value)["profilePermissions"].Where(x => (int)x["profileId"] == AppUser.ProfileId).FirstOrDefault();
                if (!profileSetting.IsNullOrEmpty())
                {
                    var hasBulkUpdatePermision = profileSetting["permissions"].Any(x => x.Value<string>() == "users");
                    customRolePermission = hasBulkUpdatePermision;
                }
            }

            if (!user.Profile.HasAdminRights && !customRolePermission)
                return;

            Role role = await _roleRepository.GetByIdAsyncWithUsers(id);
            await _roleRepository.AddOwners(role, owners);
        }

        [Route("remove_owners"), HttpPost]
        public async Task RemoveOwners([FromQuery(Name = "id")]int id, [FromBody]JArray owners)
        {
            var user = _userRepository.GetById(AppUser.Id);

            var customProfileSetting = await _settingRepository.GetByKeyAsync("custom_profile_permissions");
            var customRolePermission = false;

            if (customProfileSetting != null)
            {
                JToken profileSetting = JObject.Parse(customProfileSetting.Value)["profilePermissions"].Where(x => (int)x["profileId"] == AppUser.ProfileId).FirstOrDefault();
                if (!profileSetting.IsNullOrEmpty())
                {
                    var hasBulkUpdatePermision = profileSetting["permissions"].Any(x => x.Value<string>() == "users");
                    customRolePermission = hasBulkUpdatePermision;
                }
            }

            if (!user.Profile.HasAdminRights && !customRolePermission)
                return;

            Role role = await _roleRepository.GetByIdAsyncWithUsers(id);
            await _roleRepository.RemoveOwners(role, owners);
        }
    }
}