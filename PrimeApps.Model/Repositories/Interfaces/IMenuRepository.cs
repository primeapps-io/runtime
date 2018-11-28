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
        Task<ICollection<MenuItem>> GetAllMenuItems();
        Task<int> CreateMenuItems(MenuItem menuItem);
    }
}
