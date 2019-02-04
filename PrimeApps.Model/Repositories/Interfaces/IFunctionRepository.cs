using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IFunctionRepository : IRepositoryBaseTenant
    {
        Task<int> Count();
        Task<ICollection<Function>> Find(PaginationModel paginationModel);
        Task<Function> Get(int id);
        Task<int> Create(Function component);
        Task<int> Update(Function organization);
        Task<int> Delete(Function organization);
    }
}
