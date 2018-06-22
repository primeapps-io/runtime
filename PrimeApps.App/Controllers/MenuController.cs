using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.App.Controllers;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Entities.Application;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OfisimCRM.App.Controllers
{
    [Route("api/menu"), Authorize]
    public class MenuController : ApiBaseController
    {
        private IMenuRepository _menuRepository;
        public MenuController(IMenuRepository menuRepository)
        {
            _menuRepository = menuRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_menuRepository);

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

            var menuItemsData = await _menuRepository.GetItems(menuEntity.Id);
            //TODO Removed
            //var instance = await Workgroup.Get(AppUser.InstanceId);
            var instance = new InstanceItem();
            var profile = instance.Profiles.Single(x => x.UserIDs.Contains(AppUser.Id));
            var menuItems = new List<MenuItem>();

            foreach (var menuItem in menuItemsData)
            {
                var hasPermission = await CheckPermission(menuItem, profile, instance);

                if (hasPermission)
                    menuItems.Add(menuItem);
            }

            var menuCategories = menuItems.Where(x => !x.ParentId.HasValue && string.IsNullOrEmpty(x.Route)).ToList();

            foreach (var menuCategory in menuCategories)
            {
                var menuCategoryItems = new List<MenuItem>();

                foreach (var menuItem in menuCategory.MenuItems)
                {
                    var hasPermission = await CheckPermission(menuItem, profile, instance);

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

        private async Task<bool> CheckPermission(MenuItem menuItem, ProfileLightDTO profile, InstanceItem instance)
        {
            if (!menuItem.ModuleId.HasValue && string.IsNullOrEmpty(menuItem.Route))
                return true;

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
                        //TODO Removed
                        //var hasTimesheetPermission = await Workgroup.CheckPermission(PermissionEnum.Write, 29, EntityType.Module, AppUser.InstanceId, AppUser.LocalId);//29 is timesheet module id

                        //if (hasTimesheetPermission)
                            return true;
                        break;
                    case "timetrackers":
                        //TODO Removed
                        //var hasTimetrackersPermission = await Workgroup.CheckPermission(PermissionEnum.Write, 35, EntityType.Module, AppUser.InstanceId, AppUser.LocalId);//35 is timetrackers module id

                        //if (hasTimetrackersPermission)
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