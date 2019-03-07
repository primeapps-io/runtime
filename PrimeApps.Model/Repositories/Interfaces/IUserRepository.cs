using PrimeApps.Model.Entities.Tenant;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IUserRepository : IRepositoryBaseTenant
    {
        Task CreateAsync(TenantUser user);
        Task<ICollection<TenantUser>> GetAllAsync();
        Task<ICollection<TenantUser>> GetAllAsync(int take, int startFrom, int count);
        Task<UserInfo> GetUserInfoAsync(int userId, bool isActive = true);
        Task UpdateAsync(TenantUser usr);
        Task<TenantUser> GetById(int userId);
        TenantUser GetByIdSync(int userId);
        Task<TenantUser> GetByEmail(string email);
        Task<ICollection<TenantUser>> GetByIds(List<int> userIds);
        Task<ICollection<TenantUser>> GetByProfileIds(List<int> profileIds);
        Task<int> TerminateUser(TenantUser user);
        Task<ICollection<TenantUser>> GetNonSubscribers();
        Task<int> GetTenantUserCount();
        Task<int> Count();
        Task<ICollection<TenantUser>> Find(PaginationModel paginationModel);
    }
}