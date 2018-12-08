using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Entities.Tenant;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Helpers;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Enums;
using Newtonsoft.Json.Linq;
using OfisimCRM.App.Helpers;
using System.Web.Http;
using System.Net;

namespace PrimeApps.App.Controllers
{
	[Route("api/menu"), Authorize]
	public class MenuController : ApiBaseController
	{
		private IMenuRepository _menuRepository;
		private IProfileRepository _profileRepository;
		private ISettingRepository _settingsRepository;
		private IModuleRepository _moduleRepository;

		public MenuController(IMenuRepository menuRepository, IProfileRepository profileRepository, ISettingRepository settingsRepository, IModuleRepository moduleRepository)
		{
			_profileRepository = profileRepository;
			_menuRepository = menuRepository;
			_settingsRepository = settingsRepository;
			_moduleRepository = moduleRepository;
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_menuRepository);
			SetCurrentUser(_profileRepository);
			SetCurrentUser(_moduleRepository);

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
				menuItem.Route = menuItem.Route != null ? menuItem.Route.Replace("crm/", "") : null;
				var hasPermission = await CheckPermission(menuItem, tenantUser.Profile, instance);

				if (hasPermission)
					menuItems.Add(menuItem);
			}

			var menuCategories = menuItems.Where(x => !x.ParentId.HasValue && string.IsNullOrEmpty(x.Route)).ToList();

			foreach (var menuCategory in menuCategories)
			{
				var menuCategoryItems = new List<MenuItem>();
				menuCategory.MenuItems = menuCategory.MenuItems.Where(x => !x.Deleted).OrderBy(x => x.Order).ToList();

				foreach (var menuItem in menuCategory.MenuItems)
				{
					menuItem.Route = menuItem.Route != null ? menuItem.Route.Replace("crm/", "") : null;
					var hasPermission = await CheckPermission(menuItem, tenantUser.Profile, instance);

					if (hasPermission)
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
					case "expense":
						var hasExpensePermission = UserHelper.CheckPermission(PermissionEnum.Write, 20, EntityType.Module, currentProfile); //20 is masraflar module id
						if (hasExpensePermission)
							return true;
						break;
				}

				return false;
			}
			//TODO Removed
			var hasPermission = true;//await Workgroup.CheckPermission(PermissionEnum.Read, menuItem.ModuleId, EntityType.Module, AppUser.InstanceId, AppUser.LocalId);

			return hasPermission;
		}

		[Route("create"), HttpPost]
		public async Task<IActionResult> Create(JObject request)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			if (!request["menu"].HasValues)
				return BadRequest();

			if (request["menu"].Type != JTokenType.Array)
				return BadRequest("Please send record_ids array.");

			var defaultMenu = new JObject();

			var menu = new Menu();
			for (int i = 0; i < ((JArray)request["menu"]).Count; i++)
			{
				if ((bool)request["menu"][i]["default"])
				{
					defaultMenu["default"] = true;
					defaultMenu["profile_id"] = (int)request["menu"][i]["profileId"];
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

						else
						{
							menuItem.Deleted = true;
							await _menuRepository.UpdateMenu(menuItem);
						}
					}
				}

				menu = MenuHelper.CreateMenu((JObject)request["menu"][i]);
				var result = await _menuRepository.CreateMenu(menu);

				if (result < 1)
					throw new HttpResponseException(HttpStatusCode.InternalServerError);
			}

			return Ok(menu);
		}

		[Route("delete/{id:int}"), HttpDelete]
		public async Task<IActionResult> Delete([FromUri]int id)
		{
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
		public async Task<IActionResult> Update(int id, [FromBody]List<Menu> menuList)
		{
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
					defaultMenu["profile_id"] =menuList[i].ProfileId;
				}

				menuEntity = MenuHelper.UpdateMenu(menuList[i]);
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
		public async Task<IActionResult> CreateMenuItems(JObject request)
		{
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
				//creating Label
				//If does not have items, its main Menu 
				// step 1
				if (((JArray)request["module"][i]["items"]).Count > 0)
				{
					//step 1.1 helper
					menuItem = MenuHelper.CreateMenuItems((JObject)request["module"][i], menu, null, null);
					var result = await _menuRepository.CreateMenuItems(menuItem);

					if (result < 1)
						throw new HttpResponseException(HttpStatusCode.InternalServerError);

					for (int j = 0; j < ((JArray)request["module"][i]["items"]).Count; j++)
					{
						var moduleEntity = string.IsNullOrWhiteSpace(request["module"][i]["items"][j]["route"].ToString()) ? await _moduleRepository.GetByName(request["module"][i]["items"][j]["menuName"].ToString()) : null;
						var parent = await _menuRepository.GetMenuItemIdByName(request["module"][i]["name"].ToString(), menuItem.MenuId);
						// step 1.2 helper
						menuItem = MenuHelper.CreateMenuItems((JObject)request["module"][i]["items"][j], menu, moduleEntity, parent);

						result = await _menuRepository.CreateMenuItems(menuItem);

						if (result < 1)
							throw new HttpResponseException(HttpStatusCode.InternalServerError);
					}
				}

				//step 2
				else if ((int)request["module"][i]["parentId"] > 0)
				{
					//module->request["module"][i]["menuName"].ToString()
					var moduleEntity = string.IsNullOrWhiteSpace(request["module"][i]["route"].ToString()) ? await _moduleRepository.GetByName(request["module"][i]["menuName"].ToString()) : null;
					menu = await _menuRepository.GetByProfileId((int)request["profileId"]);
					//step 2.1 helper
					menuItem = MenuHelper.CreateMenuItems((JObject)request["module"][i], menu, moduleEntity, null);
					var result = await _menuRepository.CreateMenuItems(menuItem);

					if (result < 1)
						throw new HttpResponseException(HttpStatusCode.InternalServerError);
				}

				//step 3
				else
				{
					//If exist id, this is update label, else id is null this is new module
					var moduleType = (string)request["module"][i]["menuModuleType"];
					var module = string.Equals(moduleType, "Tanım Giriş") ? null : request["module"][i]["menuName"].ToString();
					var moduleEntity = string.IsNullOrWhiteSpace(request["module"][i]["route"].ToString()) ? await _moduleRepository.GetByName(module) : null;
					menu = await _menuRepository.GetByProfileId((int)request["profileId"]);

					if (!string.IsNullOrEmpty(module))
						menuItem = MenuHelper.CreateMenuItems((JObject)request["module"][i], menu, moduleEntity, null, true);

					//If user send only label without childs
					else
						menuItem = MenuHelper.CreateMenuItems((JObject)request["module"][i], menu, moduleEntity, null, true);

					var result = await _menuRepository.CreateMenuItems(menuItem);

					if (result < 1)
						throw new HttpResponseException(HttpStatusCode.InternalServerError);
				}
			}

			return Ok(menuItem);
		}

		[Route("update/menu_items"), HttpPut]
		public async Task<IActionResult> UpdateMenuItems(JObject request)
		{
			var menuItem = new MenuItem();
			for (int i = 0; i < ((JArray)request["menuLabel"]).Count; i++)
			{
				menuItem = await _menuRepository.GetMenuItemsById((int)request["menuLabel"][i]["id"]);
				if (menuItem == null)
					return NotFound();

				menuItem = MenuHelper.UpdateMenuItems((JObject)request["menuLabel"][i], menuItem);
				await _menuRepository.UpdateMenuItem(menuItem);
				for (int j = 0; j < ((JArray)request["menuLabel"][i]["items"]).Count; j++)
				{
					menuItem = await _menuRepository.GetMenuItemsById((int)request["menuLabel"][i]["items"][j]["id"]);
					if (menuItem == null)
						return NotFound();

					menuItem = MenuHelper.UpdateMenuItems((JObject)request["menuLabel"][i]["items"][j], menuItem);
					await _menuRepository.UpdateMenuItem(menuItem);
				}
			}
			return Ok(menuItem);
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
	}


}