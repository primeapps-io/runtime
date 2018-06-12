using System.Collections.Generic;
using System.Threading.Tasks;
using OfisimCRM.Model.Entities;

namespace OfisimCRM.Model.Repositories.Interfaces
{
    public interface IMenuRepository : IRepositoryBaseTenant
    {
        Task<Menu> GetByProfileId(int id);
        Task<Menu> GetDefault();
        Task<ICollection<Menu>> GetAll();
        Task<ICollection<MenuItem>> GetItems(int id);
        Task AddModuleToMenuAsync(Module module);
        Task DeleteModuleFromMenu(int id);
    }
}
