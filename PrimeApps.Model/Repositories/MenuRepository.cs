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
        public MenuRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

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
                .Where(x => x.MenuId == id &&
                            x.ParentId == null) //!x.Deleted && deleted oalnlar ui'da disable olarak gösterilecektir
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
                    Order = (short) (menuItems.Count + 1),
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
            var menuItems = DbContext.MenuItems.Where(x => x.Id == id); //!x.Deleted &&
            foreach (var menuItem in menuItems)
                menuItem.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task DeleteHardMenuItems(int id)
        {
            var menuItem = DbContext.MenuItems.Where(x => x.Id == id).SingleOrDefault();
            
            DbContext.MenuItems.Remove(menuItem);
             await DbContext.SaveChangesAsync();
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
                .Where(x => x.MenuId == menuId)
                .ToListAsync(); //!x.Deleted &&

            return menuItems;
        }

        public async Task<MenuItem> GetMenuItemsById(int id)
        {
            var menuItem = await DbContext.MenuItems
                .FirstOrDefaultAsync(x => x.Id == id); // !x.Deleted &&

            return menuItem;
        }

        public async Task<MenuItem> GetMenuItemIdByName(string labelName, int menuId)
        {
            var result =
                DbContext.MenuItems.FirstOrDefaultAsync(x =>
                    x.ParentId == null && x.LabelTr == labelName && x.Route == null); //!x.Deleted && 
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
            var menus = DbContext.Menus
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.Id)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Menu).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

                if (paginationModel.OrderType == "asc")
                {
                    menus = menus.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    menus = menus.OrderByDescending(x => propertyInfo.GetValue(x, null));
                }
            }

            return await menus.ToListAsync();
        }
    }
}