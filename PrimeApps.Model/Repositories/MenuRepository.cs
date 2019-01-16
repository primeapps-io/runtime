using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
	public class MenuRepository : RepositoryBaseTenant, IMenuRepository
	{
		public MenuRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

		public async Task<Menu> GetByProfileId(int id)
		{
			var menu = await DbContext.Menus
				.FirstOrDefaultAsync(x => !x.Deleted && x.ProfileId == id);

			return menu;
		}

		public async Task<Menu> GetDefault()
		{
			var menu = await DbContext.Menus
				.FirstOrDefaultAsync(x => !x.Deleted && x.Default);

			return menu;
		}

		public async Task<ICollection<Menu>> GetAll()
		{
			var menus = await DbContext.Menus
				.Where(x => !x.Deleted)
				.ToListAsync();

			return menus;
		}

		public async Task<ICollection<MenuItem>> GetItems(int id)
		{
			var menuItems = await DbContext.MenuItems
				  .Include(x => x.MenuItems).ThenInclude(z => z.CreatedBy)
				  .Include(x => x.CreatedBy)
				  .Where(x => !x.Deleted && x.MenuId == id && x.ParentId == null)
				  .OrderBy(x => x.Order).ToListAsync();


			foreach (var menu in menuItems)
			{
				var menuList = new List<MenuItem>();

				foreach (var item in menu.MenuItems)
				{
					if (item.MenuId == id)
						menuList.Add(item);
				}

				menu.MenuItems = menuList;
			}

			return menuItems;
		}

		public async Task AddModuleToMenuAsync(Module module)
		{
			var menus = await DbContext.Menus
				.Where(x => !x.Deleted)
				.ToListAsync();

			foreach (Menu menu in menus)
			{
				var menuItems = await DbContext.MenuItems
					.Where(x => !x.Deleted && x.MenuId == menu.Id && x.ParentId == null).ToListAsync();

				var menuItem = new MenuItem()
				{
					MenuId = menu.Id,
					ModuleId = module.Id,
					Route = module.Name,
					LabelEn = module.LabelEnPlural,
					LabelTr = module.LabelTrPlural,
					MenuIcon = module.MenuIcon,
					Order = (short)(menuItems.Count + 1),
					IsDynamic = true
				};

				DbContext.MenuItems.Add(menuItem);
			}

			await DbContext.SaveChangesAsync();
		}

		public async Task DeleteModuleFromMenu(int id)
		{
			var menuItems = await DbContext.MenuItems
				.Where(x => !x.Deleted && x.ModuleId == id).ToListAsync();

			foreach (var item in menuItems)
			{
				item.Deleted = true;
			}

			await DbContext.SaveChangesAsync();
		}

		public async Task<int> CreateMenu(Menu menu)
		{
			DbContext.Menus.Add(menu);
			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> DeleteSoftMenu(Menu menu)
		{
			//firs delete menu
			menu.Deleted = true;
			//then delete menu parents
			var menuItems = await GetMenuItemsByMenuId(menu.Id);
			foreach (var menuItem in menuItems)
				menuItem.Deleted = true;

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> UpdateMenu(Menu menu)
		{
			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> CreateMenuItems(MenuItem menuItem)
		{
			DbContext.MenuItems.Add(menuItem);
			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> DeleteSoftMenuItems(int id)
		{
			var menuItems = DbContext.MenuItems.Where(x => !x.Deleted && x.Id == id);
			foreach (var menuItem in menuItems)
				menuItem.Deleted = true;

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> UpdateMenuItem(MenuItem menuItem)
		{
			return await DbContext.SaveChangesAsync();
		}

		public async Task<ICollection<MenuItem>> GetAllMenuItems()
		{
			return await DbContext.MenuItems
				.Where(x => !x.Deleted).ToListAsync();
		}
		public async Task<Menu> GetById(int id)
		{
			var menu = await DbContext.Menus
				.FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

			return menu;
		}
		public async Task<ICollection<MenuItem>> GetMenuItemsByMenuId(int menuId)
		{
			var menuItems = await DbContext.MenuItems
					.Where(x => !x.Deleted && x.MenuId == menuId)
					.ToListAsync();

			return menuItems;
		}

		public async Task<MenuItem> GetMenuItemsById(int id)
		{
			var menuItem = await DbContext.MenuItems
				.FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

			return menuItem;
		}

		public async Task<MenuItem> GetMenuItemIdByName(string labelName, int menuId)
		{
			var result = DbContext.MenuItems.FirstOrDefaultAsync(x => !x.Deleted && x.ParentId == null && x.LabelTr == labelName && x.Route == null);
			return await result;
		}
		public async Task<int> Count()
		{
			var count = await DbContext.Menus
			   .Where(x => !x.Deleted).CountAsync();
			return count;
		}

		public async Task<ICollection<Menu>> Find(PaginationModel paginationModel)
		{
			var menus = GetPaginationGQuery(paginationModel)
				.Skip(paginationModel.Offset * paginationModel.Limit)
				.Take(paginationModel.Limit).ToList();

			if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
			{
				var propertyInfo = typeof(Module).GetProperty(paginationModel.OrderColumn);

				if (paginationModel.OrderType == "asc")
				{
					menus = menus.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
				}
				else
				{
					menus = menus.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
				}

			}

			return menus;

		}

		private IQueryable<Menu> GetPaginationGQuery(PaginationModel paginationModel, bool withIncludes = true)
		{
			return DbContext.Menus
				.Where(menus => !menus.Deleted);
		}


	}
}
