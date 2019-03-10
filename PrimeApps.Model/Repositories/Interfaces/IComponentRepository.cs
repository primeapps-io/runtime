using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IComponentRepository : IRepositoryBaseTenant
    {
        Task<int> Count();
        Task<ICollection<Component>> Find(PaginationModel paginationModel);
        Task<Component> Get(int id);
        Task<Component> Get(string name);
        Task<List<Component>> GetByType(ComponentType type);
        Task<int> Create(Component component);
        Task<int> Update(Component component);
        Task<int> Delete(Component component);
    }
}