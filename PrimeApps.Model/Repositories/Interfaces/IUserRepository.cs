using PrimeApps.Model.Entities.Application;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.User;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IUserRepository : IRepositoryBaseTenant
    {
        Task CreateAsync(TenantUser user);
        Task<ICollection<TenantUser>> GetAllAsync();
        Task<ICollection<TenantUser>> GetAllAsync(int take, int startFrom, int count);
        Task<UserInfo> GetUserInfoAsync(int userId);
        Task UpdateAsync(TenantUser usr);
        Task<TenantUser> GetById(int userId);
        Task<TenantUser> GetByEmail(string email);
        Task<ICollection<TenantUser>> GetByIds(List<int> userIds);
        Task<ICollection<User>> GetByProfileIds(List<int> profileIds);
        Task<int> TerminateUser(TenantUser user);
        Task<ICollection<TenantUser>> GetNonSubscribers();
    }
}
