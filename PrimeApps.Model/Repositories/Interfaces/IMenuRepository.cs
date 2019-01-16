using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
	public interface IMenuRepository : IRepositoryBaseTenant
	{
		Task<Menu> GetByProfileId(int id);
		Task<Menu> GetDefault();
		Task<ICollection<Menu>> GetAll();
		Task<ICollection<MenuItem>> GetItems(int id);
		Task AddModuleToMenuAsync(Module module);
		Task DeleteModuleFromMenu(int id);
		Task<int> CreateMenu(Menu menu);
		Task<int> DeleteSoftMenu(Menu menu);
		Task<int> UpdateMenu(Menu menu);
		Task<int> CreateMenuItems(MenuItem menuItem);
		Task<int> DeleteSoftMenuItems(int id);
		Task<int> UpdateMenuItem(MenuItem menuItem);
		Task<Menu> GetById(int id);
		Task<MenuItem> GetMenuItemsById(int id);
		Task<MenuItem> GetMenuItemIdByName(string labelName, int menuId);
		Task<ICollection<MenuItem>> GetMenuItemsByMenuId(int menuId);
		Task<ICollection<MenuItem>> GetAllMenuItems();

		Task<int> Count();
		Task<ICollection<Menu>> Find(PaginationModel paginationModel);

	}
}
