using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web.Http;
using Microsoft.AspNet.OData;
using Microsoft.AspNet.OData.Extensions;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;

namespace PrimeApps.Studio.Controllers
{
	[Route("api/menu"), Authorize]
	public class MenuController : DraftBaseController
	{
		private IMenuRepository _menuRepository;
		private IProfileRepository _profileRepository;
		private ISettingRepository _settingsRepository;
		private IModuleRepository _moduleRepository;
		private IConfiguration _configuration;
		private IPermissionHelper _permissionHelper;

		public MenuController(IMenuRepository menuRepository, IProfileRepository profileRepository,
			ISettingRepository settingsRepository, IModuleRepository moduleRepository, IConfiguration configuration,
			IPermissionHelper permissionHelper)
		{
			_profileRepository = profileRepository;
			_menuRepository = menuRepository;
			_settingsRepository = settingsRepository;
			_moduleRepository = moduleRepository;
			_configuration = configuration;
			_permissionHelper = permissionHelper;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_menuRepository, PreviewMode, TenantId, AppId);
			SetCurrentUser(_profileRepository, PreviewMode, TenantId, AppId);
			SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);

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

			//var tenantUserRepository = (IUserRepository)HttpContext.RequestServices.GetService(typeof(IUserRepository));

			var previewMode = _configuration.GetValue("AppSettings:PreviewMode", string.Empty);
			previewMode = !string.IsNullOrEmpty(previewMode) ? previewMode : "tenant";

			//if (!string.IsNullOrEmpty(previewMode))
			//{
			//tenantUserRepository.CurrentUser = new CurrentUser { UserId = AppUser.Id, TenantId = previewMode == "app" ? AppUser.AppId : AppUser.TenantId, PreviewMode = previewMode };
			//}
			//var tenantUser = tenantUserRepository.GetByIdSync(AppUser.Id);
			var menuItemsData = await _menuRepository.GetItems(menuEntity.Id);
			//TODO Removed
			//var instance = await Workgroup.Get(AppUser.InstanceId);
			var instance = new InstanceItem();


			var menuItems = new List<MenuItem>();

			foreach (var menuItem in menuItemsData)
			{
				//var hasPermission = await CheckPermission(menuItem, tenantUser.Profile, instance);

				//if (hasPermission)
				menuItems.Add(menuItem);
			}

			var menuCategories = menuItems.Where(x => !x.ParentId.HasValue && string.IsNullOrEmpty(x.Route)).ToList();

			foreach (var menuCategory in menuCategories)
			{
				var menuCategoryItems = new List<MenuItem>();
				menuCategory.MenuItems = menuCategory.MenuItems.OrderBy(x => x.Order).ToList(); //Where(x => !x.Deleted)

				foreach (var menuItem in menuCategory.MenuItems)
				{
					//var hasPermission = await CheckPermission(menuItem, tenantUser.Profile, instance);

					//if (hasPermission)
					menuCategoryItems.Add(menuItem);
				}

				menuCategory.MenuItems = menuCategoryItems;
			}

			foreach (var menuCategory in menuCategories)
			{
				if (menuCategory.MenuItems.Count < 0)
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
						currentProfile = await _profileRepository.GetProfileById(AppUser.ProfileId);
						var hasNewsFeedPermission = UserHelper.CheckPermission(PermissionEnum.Read, null,
							EntityType.Newsfeed, currentProfile);

						if (hasNewsFeedPermission)
							return true;
						break;
					case "reports":
						currentProfile = await _profileRepository.GetProfileById(AppUser.ProfileId);
						var hasReportsPermission = UserHelper.CheckPermission(PermissionEnum.Read, null,
							EntityType.Report, currentProfile);

						if (hasReportsPermission)
							return true;
						break;
					case "documentSearch":
						if (profile.DocumentSearch)
							return true;
						break;
					case "timesheet":
						currentProfile = await _profileRepository.GetProfileById(AppUser.ProfileId);
						var hasTimesheetPermission = UserHelper.CheckPermission(PermissionEnum.Write, 29,
							EntityType.Module, currentProfile); //29 is timesheet module id

						if (hasTimesheetPermission)
							return true;
						break;
					case "timetrackers":
						currentProfile = await _profileRepository.GetProfileById(AppUser.ProfileId);
						var hasTimetrackersPermission = UserHelper.CheckPermission(PermissionEnum.Write, 35,
							EntityType.Module, currentProfile); //35 is timetrackers module id

						if (hasTimetrackersPermission)
							return true;
						break;
					case "analytics":
						//TODO Removed
						if (instance.HasAnalytics.HasValue &&
							instance.HasAnalytics.Value /*&& AppUser.HasAnalyticsLicense*/)
							return true;
						break;
					case "expense":
						var hasExpensePermission = UserHelper.CheckPermission(PermissionEnum.Write, 20,
							EntityType.Module, currentProfile); //20 is masraflar module id
						if (hasExpensePermission)
							return true;
						break;
				}

				return false;
			}

			//TODO Removed
			var
				hasPermission =
					true; //await Workgroup.CheckPermission(PermissionEnum.Read, menuItem.ModuleId, EntityType.Module, AppUser.InstanceId, AppUser.LocalId);

			return hasPermission;
		}

		[Route("create"), HttpPost]
		public async Task<IActionResult> Create([FromBody] List<Menu> menuList)
		{
			if (UserProfile != ProfileEnum.Manager &&
				!_permissionHelper.CheckUserProfile(UserProfile, "menu", RequestTypeEnum.Create))
				return StatusCode(403);

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var defaultMenu = new JObject();

			for (int i = 0; i < menuList.Count; i++)
			{
				if (menuList[i].Default)
				{
					defaultMenu["default"] = true;
					defaultMenu["profile_id"] = menuList[i].ProfileId;
				}

				if (defaultMenu.Count > 0)
				{
					//check if exist default = true
					var allMenus = await _menuRepository.GetAll();
					var allMenusCopy = allMenus;
					allMenusCopy = allMenusCopy.Where(x => x.Default).ToList();
					/*Burada tüm menuleri' getiriyoruz daha önceden admin için menu oluşturulmuş olabilir*/
					if (allMenusCopy.Count > 0)
					{
						allMenus = allMenusCopy;
					}

					foreach (var menuItem in allMenus)
					{
						if (menuItem.ProfileId != (int)defaultMenu["profile_id"])
						{
							menuItem.Default = false;
							await _menuRepository.UpdateMenu(menuItem);
						}

						else
						{
							menuItem.Deleted = true;
							await _menuRepository.UpdateMenu(menuItem);
						}
					}
				}

				menuList[i] = MenuHelper.CreateMenu(menuList[i]);
				var result = await _menuRepository.CreateMenu(menuList[i]);

				if (result < 1)
					throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			return Ok(menuList);
		}

		[Route("delete/{id:int}"), HttpDelete]
		public async Task<IActionResult> Delete([FromUri] int id)
		{
			if (UserProfile != ProfileEnum.Manager &&
				!_permissionHelper.CheckUserProfile(UserProfile, "menu", RequestTypeEnum.Delete))
				return StatusCode(403);

			if (id < 0)
				return BadRequest("id is required");

			var menuEntity = await _menuRepository.GetById(id);

			if (menuEntity == null)
				return NotFound();

			//first delete menu
			await _menuRepository.DeleteSoftMenu(menuEntity);

			return Ok(menuEntity);
		}


		[Route("update/{id:int}"), HttpPut]
		public async Task<IActionResult> Update(int id, [FromBody] List<Menu> menuList)
		{
			if (UserProfile != ProfileEnum.Manager &&
				!_permissionHelper.CheckUserProfile(UserProfile, "menu", RequestTypeEnum.Update))
				return StatusCode(403);

			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (id < 0)
				return BadRequest("id is required");

			var menuEntity = await _menuRepository.GetById(id);

			if (menuEntity == null)
				return NotFound();

			var defaultMenu = new JObject();

			for (int i = 0; i < menuList.Count; i++)
			{
				if (menuList[i].Default)
				{
					defaultMenu["default"] = true;
					defaultMenu["profile_id"] = menuList[i].ProfileId;
				}

				menuEntity = MenuHelper.UpdateMenu(menuList[i], menuEntity);
				await _menuRepository.UpdateMenu(menuEntity);
			}

			if (defaultMenu.Count > 0)
			{
				//check if exist default = true
				var allMenus = await _menuRepository.GetAll();

				foreach (var menuItem in allMenus.Where(x => x.Default))
				{
					if (menuItem.ProfileId != (int)defaultMenu["profile_id"])
					{
						menuItem.Default = false;
						await _menuRepository.UpdateMenu(menuItem);
					}
				}
			}

			return Ok(menuEntity);
		}

		[Route("create/menu_items"), HttpPost]
		public async Task<IActionResult> CreateMenuItems([FromBody] JObject request)
		{
			if (UserProfile != ProfileEnum.Manager &&
				!_permissionHelper.CheckUserProfile(UserProfile, "menu", RequestTypeEnum.Create))
				return StatusCode(403);
			/**
			 * moduleItem: modül
			 * profileItem: profil 
			 * menuItem:label-Tanım Giriş
			 */
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (!request["module"].HasValues)
				return BadRequest();

			if (request["module"].Type != JTokenType.Array)
				return BadRequest("Please send module array.");

			if ((int)request["profileId"] < 0)
				return BadRequest("profileID is required");

			var menu = await _menuRepository.GetByProfileId((int)request["profileId"]);
			var menuItem = new MenuItem();

			for (int i = 0; i < ((JArray)request["module"]).Count; i++)
			{
				//isDynamic -> true ise kullanıcının create ettiği modüldür
				var moduleEntity = (bool)request["module"][i]["isDynamic"] ? await _moduleRepository.GetByName(request["module"][i]["menuName"].ToString()) : null;//string.IsNullOrWhiteSpace(request["module"][i]["nodes"][j]["route"].ToString()) ?
																																								   //Gelen module'de oalbilir
				menuItem = MenuHelper.CreateMenuItems((JObject)request["module"][i], menu, moduleEntity, null);
				var result = await _menuRepository.CreateMenuItems(menuItem);

				if (result < 1)
					throw new HttpResponseException(HttpStatusCode.InternalServerError);

				var parent = menuItem;
				for (int j = 0; j < ((JArray)request["module"][i]["nodes"]).Count; j++)
				{
					if ((int)request["module"][i]["nodes"][j]["id"] < 1)
					{
						moduleEntity = (bool)request["module"][i]["nodes"][j]["isDynamic"] ? await _moduleRepository.GetByName(request["module"][i]["nodes"][j]["menuName"].ToString()) : null;//string.IsNullOrWhiteSpace(request["module"][i]["nodes"][j]["route"].ToString()) ?
						menuItem = MenuHelper.CreateMenuItems((JObject)request["module"][i]["nodes"][j], menu, moduleEntity, parent);

						result = await _menuRepository.CreateMenuItems(menuItem);

						if (result < 1)
							throw new HttpResponseException(HttpStatusCode.InternalServerError);
					}
					//Parent create edilip, daha önceden eklenmiş olan moduller bu parent altına eklenmişse update edilecek
					else
					{
						//ParentId'sini güncelliyoruz
						request["module"][i]["nodes"][j]["parentId"] = parent.Id;
						menuItem = await _menuRepository.GetMenuItemsById((int)request["module"][i]["nodes"][j]["id"]);
						if (menuItem == null)
							return NotFound();
						menuItem = MenuHelper.UpdateMenuItems((JObject)request["module"][i]["nodes"][j], menuItem);
						await _menuRepository.UpdateMenuItem(menuItem);
					}

				}
			}

			return Ok(menuItem);
		}

		[Route("delete/menu_items"), HttpDelete]
		public async Task<IActionResult> DeleteMenuItems([FromBody] int[] ids)
		{
			if (UserProfile != ProfileEnum.Manager &&
				!_permissionHelper.CheckUserProfile(UserProfile, "menu", RequestTypeEnum.Delete))
				return StatusCode(403);

			var menuItemsEntity = new MenuItem();
			foreach (var id in ids)
			{
				if (id < 0)
					return BadRequest("id is required");

				menuItemsEntity = await _menuRepository.GetMenuItemsById(id);
				if (menuItemsEntity == null)
					return NotFound();

				await _menuRepository.DeleteHardMenuItems(id);
			}

			return Ok();
		}

		[Route("update/menu_items"), HttpPut]
		public async Task<IActionResult> UpdateMenuItems([FromBody] JObject request)
		{
			if (UserProfile != ProfileEnum.Manager &&
				!_permissionHelper.CheckUserProfile(UserProfile, "menu", RequestTypeEnum.Update))
				return StatusCode(403);

			var menuItem = new MenuItem();
			for (int i = 0; i < ((JArray)request["menuLabel"]).Count; i++)
			{
				menuItem = await _menuRepository.GetMenuItemsById((int)request["menuLabel"][i]["id"]);
				if (menuItem == null)
					return NotFound();

				menuItem = MenuHelper.UpdateMenuItems((JObject)request["menuLabel"][i], menuItem);
				await _menuRepository.UpdateMenuItem(menuItem);
				//eğer daha önceden mevcut olan parent'ın childları varsa, childları update et

				for (int j = 0; j < ((JArray)request["menuLabel"][i]["nodes"]).Count; j++)
				{
					menuItem = await _menuRepository.GetMenuItemsById((int)request["menuLabel"][i]["nodes"][j]["id"]);
					if (menuItem == null)
						return NotFound();

					menuItem = MenuHelper.UpdateMenuItems((JObject)request["menuLabel"][i]["nodes"][j], menuItem);
					await _menuRepository.UpdateMenuItem(menuItem);
				}
			}

			return Ok(request);
		}

		[Route("get_menu/{id:int}"), HttpGet]
		public async Task<Menu> GetMenuById(int id)
		{
			return await _menuRepository.GetById(id);
		}

		[Route("get_all"), HttpGet]
		public async Task<ICollection<Menu>> GetAllMenus()
		{
			return await _menuRepository.GetAll();
		}

		[Route("count"), HttpGet]
		public async Task<IActionResult> Count()
		{
			var count = await _menuRepository.Count();

			if (count < 1)
				return Ok(null);

			return Ok(count);
		}

        [Route("find")]
        public IActionResult Find(ODataQueryOptions<Menu> queryOptions)
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "menu", RequestTypeEnum.View))
                return StatusCode(403);

            var views = _menuRepository.Find();
            var queryResults = (IQueryable<Menu>)queryOptions.ApplyTo(views, new ODataQuerySettings() { EnsureStableOrdering = false });
            return Ok(new PageResult<Menu>(queryResults, Request.ODataFeature().NextLink, Request.ODataFeature().TotalCount));
        }


        [Route("get_menu_items/{id:int}"), HttpGet]
		public async Task<IActionResult> GetMenuItemsByMenuId([FromUri] int menuId)
		{
			var menuItems = await _menuRepository.GetMenuItemsByMenuId(menuId);

			if (menuItems == null)
				return Ok(null);

			return Ok(menuItems);
		}
	}
}