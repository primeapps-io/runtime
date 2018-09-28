using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
                  .Where(x => !x.Deleted && x.MenuId == id && x.ParentId == null).ToListAsync();


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
    }
}
