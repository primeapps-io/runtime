using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IUserGroupRepository : IRepositoryBaseTenant
    {
        Task<UserGroup> GetById(int id);
        Task<UserGroup> GetByName(string name);
        Task<ICollection<UserGroup>> GetAll();
        Task<int> Create(UserGroup note);
        Task<int> Update(UserGroup note);
        Task<int> DeleteSoft(UserGroup note);
        Task<int> DeleteHard(UserGroup note);
    }
}
