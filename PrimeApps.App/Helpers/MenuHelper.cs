using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;

namespace OfisimCRM.App.Helpers
{
	public static class MenuHelper
	{
		public static Menu CreateMenu(Menu menu, bool deleted = false)
		{
			var newMenu = new Menu()
			{
				Name = menu.Name,
				ProfileId = menu.ProfileId,
				Default = menu.Default,
				Deleted = menu.Deleted,
				Description = menu.Description,
			};

			return newMenu;
		}
		public static Menu UpdateMenu(Menu menu, Menu updateMenu)
		{

			updateMenu.Name = menu.Name;
			updateMenu.Description = menu.Description;
			updateMenu.Default = menu.Default;
			updateMenu.ProfileId = menu.ProfileId;

			return updateMenu;
		}

		public static MenuItem CreateMenuItems(JObject request, Menu menu, Module moduleEntity, MenuItem parent, bool step3 = false)
		{

			var module = "";
			var route = "";
			var moduleName = "";//Görünen modül name
			var labelName = "";
			var parentId = new int();
			var menuItem = new MenuItem();

			//first 1.1
			if (menu != null && moduleEntity == null && parent == null && string.IsNullOrEmpty((string)request["route"]) && !step3)
			{
				labelName = request["name"].ToString();

				menuItem = new MenuItem()
				{
					MenuId = menu.Id,
					ModuleId = null,
					ParentId = null,
					Route = null,
					LabelEn = labelName,
					LabelTr = labelName,
					MenuIcon = request["icon"].ToString(),
					Order = (short)(int)request["no"],
					IsDynamic = false,
					Deleted = false
				};
			}

			//step 1.2
			else if ((int)request["menuId"] > 0 && parent != null)//menu != null && moduleEntity != null && 
			{
				//If exist route, this is custom menu and route is crm/module exp: crm/dashboards or crm/reports
				route = request["route"].ToString();
				moduleName = request["name"].ToString();
				module = request["menuName"].ToString();
				//If send dont chose items, we will chek it, if child dont have name continue
				if (!string.IsNullOrEmpty(moduleName))
				{
					parentId = parent.Id;
					bool value = moduleEntity != null ? (moduleEntity.SystemType.HasFlag(SystemType.System) ? false : true) : false;
					menuItem = new MenuItem()
					{
						MenuId = menu.Id,
						ModuleId = moduleEntity != null ? moduleEntity?.Id : null,
						ParentId = parentId,
						Route = value ? moduleEntity.Name : string.IsNullOrWhiteSpace(route) ? "modules/" + moduleEntity.Name : "crm/" + module,
						LabelEn = moduleName,
						LabelTr = moduleName,
						MenuIcon = moduleEntity != null ? moduleEntity.MenuIcon : request["icon"].ToString(),
						Order = (short)(int)request["no"],
						IsDynamic = value,
						Deleted = false
					};
				}
			}

			//step 2.1
			if ((int)request["parentId"] > 0 && !step3)
			{
				route = request["route"].ToString();
				module = request["menuName"].ToString();
				moduleName = request["name"].ToString();
				bool value = moduleEntity != null ? (moduleEntity.SystemType.HasFlag(SystemType.System) ? false : true) : false;

				menuItem = new MenuItem()
				{
					MenuId = menu.Id,
					ModuleId = moduleEntity != null ? moduleEntity?.Id : null,
					ParentId = (int)request["parentId"],
					Route = value ? moduleEntity.Name : string.IsNullOrWhiteSpace(route) ? "modules/" + moduleEntity.Name : "crm/" + module,
					LabelEn = string.IsNullOrWhiteSpace(moduleName) ? null : moduleName,
					LabelTr = string.IsNullOrWhiteSpace(moduleName) ? null : moduleName,
					MenuIcon = moduleEntity != null ? moduleEntity.MenuIcon : request["icon"].ToString(),
					Order = (short)(int)request["no"],
					IsDynamic = value,
					Deleted = false
				};
			}

			//step 3
			if (menu != null && parent == null && step3)
			{
				route = request["route"].ToString();
				var moduleType = (string)request["menuModuleType"];
				moduleName = request["name"].ToString();
				var menuName = string.Equals(moduleType, "Tanım Giriş") ? "" : request["menuName"].ToString();
				bool value = moduleEntity != null ? (moduleEntity.SystemType.HasFlag(SystemType.System) ? false : true) : false;
				menuItem = new MenuItem()
				{
					MenuId = menu.Id,
					ModuleId = moduleEntity != null ? moduleEntity?.Id : null,
					ParentId = null,
					//If menuName is null this is label and dont have a route
					Route = value ? moduleEntity.Name : string.IsNullOrWhiteSpace(menuName) ? route : string.IsNullOrWhiteSpace(route) ? "modules/" + moduleEntity.Name : "crm/" + menuName,
					LabelEn = string.IsNullOrWhiteSpace(moduleName) ? null : moduleName,
					LabelTr = string.IsNullOrWhiteSpace(moduleName) ? null : moduleName,
					MenuIcon = moduleEntity != null ? moduleEntity.MenuIcon : request["icon"].ToString(),
					Order = (short)(int)request["no"],
					IsDynamic = value,
					Deleted = false
				};
			}

			return menuItem;
		}

		public static MenuItem UpdateMenuItems(JObject request, MenuItem menuItem)
		{
			menuItem.MenuId = menuItem.MenuId;
			menuItem.ModuleId = menuItem.ModuleId;
			menuItem.ParentId = menuItem.ParentId;
			menuItem.Route = menuItem.Route;
			menuItem.LabelEn = (string)request["name"];
			menuItem.LabelTr = (string)request["name"];
			menuItem.MenuIcon = (string)request["icon"];
			menuItem.Order = (short)(int)request["no"];
			menuItem.IsDynamic = false;

			return menuItem;
		}
	}
}