using PrimeApps.Model.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IUserCustomShareRepository : IRepositoryBaseTenant
    {
        Task<ICollection<UserCustomShare>> GetAllBasic();
        Task<UserCustomShare> GetByUserId(int id);
        Task<int> Create(UserCustomShare userowner);
        Task<UserCustomShare> GetByIdBasic(int id);
        Task<UserCustomShare> GetById(int id);
        Task<int> Update(UserCustomShare userowner);
        Task<int> DeleteSoft(UserCustomShare userowner);
    }
}
