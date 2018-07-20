using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Entities.Application;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Helpers;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Controllers
{
    [Route("api/menu"), Authorize]
    public class MenuController : ApiBaseController
    {
        private IMenuRepository _menuRepository;
		private IProfileRepository _profileRepository;
		private ISettingRepository _settingsRepository;

        public MenuController(IMenuRepository menuRepository, IProfileRepository profileRepository, ISettingRepository settingsRepository)
        {
			_profileRepository = profileRepository;
			_menuRepository = menuRepository;
			_settingsRepository = settingsRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_menuRepository);
            SetCurrentUser(_profileRepository);

            base.OnActionExecuting(context);
        }

        [Route("get/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var menuEntity = await _menuRepository.GetByProfileId(id);

            if (menuEntity == null)
                menuEntity = await _menuRepository.GetDefault();

            if (menuEntity == null)
                return Ok();

            var tenantUserRepository = (IUserRepository)HttpContext.RequestServices.GetService(typeof(IUserRepository));
            tenantUserRepository.CurrentUser = new CurrentUser { UserId = AppUser.Id, TenantId = AppUser.TenantId };
            var tenantUser = tenantUserRepository.GetByIdSync(AppUser.Id);
            var menuItemsData = await _menuRepository.GetItems(menuEntity.Id);
            //TODO Removed
            //var instance = await Workgroup.Get(AppUser.InstanceId);
            var instance = new InstanceItem();
           
           
            var menuItems = new List<MenuItem>();

            foreach (var menuItem in menuItemsData)
            {
                var hasPermission = await CheckPermission(menuItem, tenantUser.Profile, instance);

                if (hasPermission)
                    menuItems.Add(menuItem);
            }

            var menuCategories = menuItems.Where(x => !x.ParentId.HasValue && string.IsNullOrEmpty(x.Route)).ToList();

            foreach (var menuCategory in menuCategories)
            {
                var menuCategoryItems = new List<MenuItem>();

                foreach (var menuItem in menuCategory.MenuItems)
                {
                    var hasPermission = await CheckPermission(menuItem, tenantUser.Profile, instance);

                    if (hasPermission)
                        menuCategoryItems.Add(menuItem);
                }

                menuCategory.MenuItems = menuCategoryItems;
            }

            foreach (var menuCategory in menuCategories)
            {
                if (menuCategory.MenuItems.Count < 1)
                    menuItems.Remove(menuCategory);
            }

            return Ok(menuItems);
        }

        private async Task<bool> CheckPermission(MenuItem menuItem, Profile profile, InstanceItem instance)
        {
            if (!menuItem.ModuleId.HasValue && string.IsNullOrEmpty(menuItem.Route))
                return true;
			Profile currentProfile = null;
            if (!menuItem.ModuleId.HasValue && !string.IsNullOrEmpty(menuItem.Route))
            {
                switch (menuItem.Route)
                {
                    case "dashboard":
                        if (profile.Dashboard)
                            return true;
                        break;
                    case "home":
                        if (profile.Home)
                            return true;
                        break;
                    case "tasks":
                        if (profile.Tasks)
                            return true;
                        break;
                    case "calendar":
                        if (profile.Calendar)
                            return true;
                        break;
                    case "newsfeed":
                        if (profile.Newsfeed)
                            return true;
                        break;
                    case "reports":
                        if (profile.Report)
                            return true;
                        break;
                    case "documentSearch":
                        if (profile.DocumentSearch)
                            return true;
                        break;
                    case "timesheet":
						currentProfile = await _profileRepository.GetProfileById(AppUser.ProfileId);
                        var hasTimesheetPermission = UserHelper.CheckPermission(PermissionEnum.Write, 29, EntityType.Module, currentProfile);//29 is timesheet module id

                        if (hasTimesheetPermission)
                            return true;
                        break;
                    case "timetrackers":
						currentProfile = await _profileRepository.GetProfileById(AppUser.ProfileId);
						var hasTimetrackersPermission = UserHelper.CheckPermission(PermissionEnum.Write, 35, EntityType.Module, currentProfile);//35 is timetrackers module id

						if (hasTimetrackersPermission)
							return true;
                        break;
                    case "analytics":
						//TODO Removed
						if (instance.HasAnalytics.HasValue && instance.HasAnalytics.Value /*&& AppUser.HasAnalyticsLicense*/)
                            return true;
                        break;
                }

                return false;
            }
            //TODO Removed
            var hasPermission = true;//await Workgroup.CheckPermission(PermissionEnum.Read, menuItem.ModuleId, EntityType.Module, AppUser.InstanceId, AppUser.LocalId);

            return hasPermission;
        }
    }
}