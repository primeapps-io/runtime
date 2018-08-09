﻿using PrimeApps.Model.Entities.Application;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Role;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IRoleRepository : IRepositoryBaseTenant
    {
        Task AddOwnersRecursiveAsync(Role role, ICollection<string> owners, int? tenantId = null, int? oldParentRoleID = null);
        Task AddUserAsync(int userId, int roleID, bool saveChanges = true, int? tenantId = null);
        Task CreateAsync(Role newRole);
        Task GenerateDefaultRolesAsync(int userId);
        Task<IEnumerable<RoleDTO>> GetAllAsync();
        Task<Role> GetByIdAsync(int id);
        Task<Role> GetByIdAsyncWithUsers(int id);
        Task<IList<Role>> GetRoleTreeRecursiveAsync(Role root);
        Task<IEnumerable<RoleInfo>> GetUserRolesForCache();
        Task RemoveAsync(int roleID, int replacementRoleId);
        Task RemoveOwnersRecursiveAsync(Role role, ICollection<string> owners);
        Task RemoveUserAsync(int userId, int roleID);
        Task UpdateAsync(Role roleToUpdate, RoleDTO role);
    }
}
