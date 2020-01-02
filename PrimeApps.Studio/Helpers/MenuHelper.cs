using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Studio.Helpers
{
	public static class MenuHelper
	{
		public static Menu CreateMenu(Menu menu)
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

		public static MenuItem CreateMenuItems(JObject request, Menu menu, Module moduleEntity, MenuItem parent)
		{
			var menuItem = new MenuItem()
			{
				MenuId = menu.Id,
				ModuleId = moduleEntity != null ? moduleEntity?.Id : null,
				ParentId = parent != null ? parent?.Id : null,
				Route = moduleEntity != null ? moduleEntity.Name : request["route"].ToString(),
				LabelEn = request["name"].ToString(),
				LabelTr = request["name"].ToString(),
				MenuIcon = request["icon"].ToString(),
				Order = (short)(int)request["no"],
				IsDynamic = (bool)request["isDynamic"],
				Deleted = (bool)request["disabled"]
			};

			return menuItem;
		}

		public static MenuItem UpdateMenuItems(JObject request, MenuItem menuItem, string language)
		{
			menuItem.MenuId = menuItem.MenuId;
			menuItem.ModuleId = menuItem.ModuleId;
			menuItem.ParentId = (int)request["parentId"] > 0 ? (int?)request["parentId"] : null;
			menuItem.Route = (string)request["menuName"]; //menuItem.Route;
			if (language == "en")
				menuItem.LabelEn = (string)request["name"];
			else
				menuItem.LabelTr = (string)request["name"];

			menuItem.MenuIcon = (string)request["icon"];
			menuItem.Order = (short)(int)request["no"];
			menuItem.IsDynamic = (bool)request["isDynamic"];
			menuItem.Deleted = (bool)request["disabled"];

			return menuItem;
		}
	}
}