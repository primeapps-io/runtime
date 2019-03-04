using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IScriptRepository : IRepositoryBaseTenant
    {
        Task<int> Count();
        Task<Component> Get(int id);
        Task<ICollection<Component>> Find(PaginationModel paginationModel);
        Task<List<Component>> GetByPlace(ComponentPlace place);
        Task<Component> GetGlobalSettings();
        Task<int> Create(Component component);
        Task<int> Update(Component component);
        Task<int> Delete(Component component);
    }
}