using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Role;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
    public class RoleRepository : RepositoryBaseTenant, IRoleRepository
    {
        private Warehouse _warehouse;
        private IConfiguration _configuration;

        public RoleRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
            _configuration = configuration;
        }

        public RoleRepository(TenantDBContext dbContext, Warehouse warehouse, IConfiguration configuration) : base(dbContext, configuration)
        {
            _warehouse = warehouse;
            _configuration = configuration;
        }

        /// <summary>
        /// Creates a new user role.
        /// </summary>
        /// <param name="newRole"></param>
        /// <returns></returns>
        public async Task CreateAsync(Role newRole)
        {
            DbContext.Roles.Add(newRole);
            await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets all roles in the tenant database.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<RoleDTO>> GetAllAsync()
        {
            return await DbContext.Roles.Select(x => new RoleDTO()
            {
                Id = x.Id,
                DescriptionEn = x.DescriptionEn,
                DescriptionTr = x.DescriptionTr,
                LabelEn = x.LabelEn,
                LabelTr = x.LabelTr,
                Master = x.Master,
                OwnersRaw = x.Owners,
                ReportsTo = x.ReportsToId,
                ShareData = x.ShareData,
                CreatedBy = x.CreatedById,
                Users = x.Users.Select(y => y.Id).ToList()
            }).ToListAsync();
        }

        /// <summary>
        /// Creates pre-defined roles for a new tenant database.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task GenerateDefaultRolesAsync(int userId)
        {

            Role roleCeo = new Role()
            {

                LabelEn = "CEO",
                LabelTr = "Genel Müdür",
                DescriptionEn = "User belonging to this role can access data of all other users.",
                DescriptionTr = "Bu role sahip kullanıcı diğer tüm kullanıcıların verilerine erişebilir.",
                Master = true,
                ReportsTo = null,
                CreatedById = userId
            };


            Role roleManager = new Role()
            {

                LabelEn = "Manager",
                LabelTr = "Yönetici",
                DescriptionEn = "User belonging to this role can access own data and data of all other users. This role cannot access CEO's data.",
                DescriptionTr = "Bu role sahip kullanıcı kendisinin ve bağlı olan alt rollerin verilerine erişebilir. Genel Müdür rolünün verilerine erişemez.",
                Master = false,
                ReportsTo = roleCeo,
                CreatedById = userId
            };


            Role roleEmployee = new Role()
            {

                LabelEn = "Employee",
                LabelTr = "Uzman",
                DescriptionEn = "User belonging to this role can access only own data.",
                DescriptionTr = "Bu role sahip kullanıcı sadece kendisinin verilerine erişebilir.",
                Master = false,
                ReportsTo = roleManager,
                CreatedById = userId
            };

            roleCeo.OwnersList.Add(userId.ToString());

            DbContext.Roles.Add(roleCeo);
            DbContext.Roles.Add(roleManager);
            DbContext.Roles.Add(roleEmployee);


            await DbContext.SaveChangesAsync();



        }

        /// <summary>
        /// Gets all user roles with a compatible formatting for cache.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<RoleInfo>> GetUserRolesForCache()
        {
            return await DbContext.Users
                .Select(x => new RoleInfo()
                {
                    UserId = x.Id,
                    RoleId = x.RoleId.Value
                }).ToListAsync();
        }

        /// <summary>
        /// Adds an existing user to a specific role in the tenant database.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleID"></param>
        /// <returns></returns>
        public async Task AddUserAsync(int userId, int roleID, bool saveChanges = true, int? tenantId = null)
        {
            var choosenRole = await DbContext.Roles.FindAsync(roleID);
            var user = await DbContext.Users.FindAsync(userId);

            if (tenantId.HasValue)
                choosenRole.UpdatedById = tenantId;

            user.Role = choosenRole;

            string userIdString = userId.ToString();
            ICollection<string> userIdArray = new List<string>() { userIdString };

            if (choosenRole.ShareData)
                choosenRole.OwnersList.Add(userIdString);

            await AddOwnersRecursiveAsync(choosenRole, userIdArray, tenantId);

            if (saveChanges)
            {
                var result = await DbContext.SaveChangesAsync();
                if (result > 0 && string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
                {
                    var platformTenantId = int.Parse(_configuration.GetSection("AppSettings")["PrimeAppsTenantId"]);

                    if (_warehouse.DatabaseName != "0")
                        BackgroundJob.Enqueue(() => _warehouse.UpdateTenantUser(userId, _warehouse.DatabaseName, CurrentUser.TenantId, platformTenantId));
                }
            }
        }

        /// <summary>
        /// Removes an existing user from a specific role in the tenant database.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="roleID"></param>
        public async Task RemoveUserAsync(int userId, int roleID)
        {
            var choosenRole = await DbContext.Roles.FindAsync(roleID);
            var user = await DbContext.Users.FindAsync(UserId);
            string userIdString = userId.ToString();
            ICollection<string> userIdArray = new List<string>() { userIdString };

            choosenRole.OwnersList.Remove(userIdString);
            await RemoveOwnersRecursiveAsync(choosenRole, userIdArray);

            await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Removes Role and transfers all child users and roles to another role.
        /// </summary>
        /// <param name="roleID"></param>
        /// <param name="replacementRoleId"></param>
        /// <returns></returns>
        public async Task RemoveAsync(int roleID, int replacementRoleId)
        {
            Role roleToDelete = await DbContext.Roles.Include(x => x.Users).SingleOrDefaultAsync(x => x.Id == roleID),
                replacementRole = await DbContext.Roles.SingleOrDefaultAsync(x => x.Id == replacementRoleId),
                firstChild = await DbContext.Roles.SingleOrDefaultAsync(x => x.ReportsToId == roleToDelete.Id);

            foreach (TenantUser user in roleToDelete.Users)
            {
                /// change role id for all users in the role to be deleted.
                user.RoleId = replacementRole.Id;
            }

            if (firstChild != null)
            {
                /// if there is a child node under the role, move it under replacement role.
                firstChild.ReportsToId = replacementRole.Id;
            }

            /// remove all owners belonging this node from the parent node and add them to the new parent node if exists.
            var userIdList = roleToDelete.Users.Select(x => x.Id.ToString()).ToList();
            await RemoveOwnersRecursiveAsync(roleToDelete, userIdList);
            await AddOwnersRecursiveAsync(replacementRole, userIdList);

            DbContext.Roles.Remove(roleToDelete);

            await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Finds all roles reporting to the root recursively.
        /// </summary>
        /// <param name="root"></param>
        /// <returns></returns>
        public async Task<IList<Role>> GetRoleTreeRecursiveAsync(Role root)
        {
            List<Role> childRoles = new List<Role>(),
                       recursiveRoles = new List<Role>();
            childRoles.AddRange(await DbContext.Roles.Where(x => x.ReportsToId == root.Id).ToListAsync());

            foreach (var role in childRoles)
            {
                recursiveRoles.AddRange(await GetRoleTreeRecursiveAsync(role));
            }
            childRoles.AddRange(recursiveRoles);
            return childRoles;

        }

        /// <summary>
        /// This method updates owners hierarchically for all roles related to parent role. It does not reflect changes to the database! Call update database command in your algorithm exclusively.
        /// </summary>
        /// <param name="role"></param>
        /// <returns></returns>
        public async Task RemoveOwnersRecursiveAsync(Role role, ICollection<string> owners)
        {
            Role parentRole = await DbContext.Roles.FindAsync(role.ReportsToId);

            if (parentRole != null)
            {
                parentRole.OwnersList.RemoveAll(x => owners.Contains(x));

                await RemoveOwnersRecursiveAsync(parentRole, owners);
            }

        }

        public async Task AddOwnersRecursiveAsync(Role role, ICollection<string> owners, int? tenantId = null)
        {
            Role parentRole = await DbContext.Roles.FindAsync(role.ReportsToId);

            if (parentRole != null)
            {
                parentRole.OwnersList.AddRange(owners);
                parentRole.OwnersList = parentRole.OwnersList.Distinct().ToList();

                if (tenantId.HasValue)
                    parentRole.UpdatedById = tenantId;

                await AddOwnersRecursiveAsync(parentRole, owners, tenantId);
            }
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            return await DbContext.Roles.FindAsync(id);
        }

        public async Task<Role> GetByIdAsyncWithUsers(int id)
        {
            return await DbContext.Roles.Include(x => x.Users).FirstOrDefaultAsync(x => x.Id == id);
        }

        /// <summary>
        /// Updates a role with its sub roles.
        /// </summary>
        /// <param name="updatedRole"></param>
        /// <returns></returns>
        public async Task UpdateAsync(Role roleToUpdate, RoleDTO role)
        {
            roleToUpdate.Id = role.Id;
            roleToUpdate.DescriptionEn = role.DescriptionEn;
            roleToUpdate.DescriptionTr = role.DescriptionTr;
            roleToUpdate.LabelEn = role.LabelEn;
            roleToUpdate.LabelTr = role.LabelTr;
            roleToUpdate.ShareData = role.ShareData;

            if (role.ReportsTo != roleToUpdate.ReportsToId)
            {
                /// parent role of the role has been changed.
                if (roleToUpdate.ReportsToId.HasValue)
                {
                    /// the role had a parent role before, so just go trough the parent branch and update until the root.
                    await RemoveOwnersRecursiveAsync(roleToUpdate, roleToUpdate.OwnersList);
                }

                /// update the roles reports to id (parent)
                roleToUpdate.ReportsToId = role.ReportsTo;

                if (roleToUpdate.ReportsToId.HasValue)
                {
                    /// There is a new parent, so add user id to owners list of the parent roles until the root.
                    await AddOwnersRecursiveAsync(roleToUpdate, roleToUpdate.OwnersList);
                }
            }

            if (roleToUpdate.ShareData)
            {
                foreach (var user in roleToUpdate.Users)
                {
                    if (!roleToUpdate.OwnersList.Contains(user.Id.ToString()))
                        roleToUpdate.OwnersList.Add(user.Id.ToString());
                }
            }
            else
            {
                foreach (var user in roleToUpdate.Users)
                {
                    if (roleToUpdate.OwnersList.Contains(user.Id.ToString()))
                        roleToUpdate.OwnersList.Remove(user.Id.ToString());
                }
            }

            await DbContext.SaveChangesAsync();
        }
    }
}
