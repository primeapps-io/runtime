using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Context;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Common.Role;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class UserRepository : RepositoryBaseTenant, IUserRepository
    {
        private Warehouse _warehouse;

        public UserRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public UserRepository(TenantDBContext dbContext, Warehouse warehouse, IConfiguration configuration) : base(dbContext, configuration)
        {
            _warehouse = warehouse;
        }

        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<int> CreateAsync(TenantUser user)
        {
            DbContext.Users.Add(user);

            return await DbContext.SaveChangesAsync();

            //if (result > 0 && !string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
            //{
            //    if (_warehouse != null && _warehouse.DatabaseName != "0")
            //        BackgroundJob.Enqueue(() => _warehouse.CreateTenantUser(user.Id, _warehouse.DatabaseName, CurrentUser, user.Culture.Contains("tr") ? "tr" : "en"));
            //}
        }

        /// <summary>
        /// Gets all users in current tenant database.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<TenantUser>> GetAllAsync()
        {
            return await DbContext.Users.ToListAsync();
        }

        /// <summary>
        /// Gets all users in current tenant database by paging.
        /// </summary>
        /// <param name="take"></param>
        /// <param name="startFrom"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task<ICollection<TenantUser>> GetAllAsync(int take, int startFrom, int count)
        {
            return await DbContext.Users.Take(count).Skip(startFrom).ToListAsync();
        }

        /// <summary>
        /// Gets required user info for the application start-up.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<UserInfo> GetUserInfoAsync(int userId, bool isActive = true, string language = "en")
        {
            var userInfo = await DbContext.Users.Where(x => x.Id == userId && x.IsActive == isActive)
                .Include(x => x.Profile)
                .Include(x => x.Role)
                .Include(x => x.Groups)
                .ThenInclude(x => x.User)
                .Select(user => new UserInfo()
                {
                    picture = user.Picture,
                    email = user.Email,
                    firstName = user.FirstName,
                    lastName = user.LastName,
                    fullName = user.FullName,
                    phone = user.Phone,
                    ID = user.Id,
                    currency = user.Currency,
                    createdAt = user.CreatedAt,
                    profile = new ProfileDTO()
                    {
                        Id = user.Profile.Id,
                        DescriptionEn = user.Profile.DescriptionEn,
                        DescriptionTr = user.Profile.DescriptionTr,
                        NameEn = user.Profile.NameEn,
                        NameTr = user.Profile.NameTr,
                        HasAdminRights = user.Profile.HasAdminRights,
                        IsPersistent = user.Profile.IsPersistent,
                        SendSMS = user.Profile.SendSMS,
                        SendEmail = user.Profile.SendEmail,
                        ExportData = user.Profile.ExportData,
                        ImportData = user.Profile.ImportData,
                        WordPdfDownload = user.Profile.WordPdfDownload,
                        LeadConvert = user.Profile.LeadConvert,
                        DocumentSearch = user.Profile.DocumentSearch,
                        Tasks = user.Profile.Tasks,
                        Calendar = user.Profile.Calendar,
                        Newsfeed = user.Profile.Newsfeed,
                        Report = user.Profile.Report,
                        Dashboard = user.Profile.Dashboard,
                        Home = user.Profile.Home,
                        CollectiveAnnualLeave = user.Profile.CollectiveAnnualLeave,
                        StartPage = user.Profile.StartPage,
                        SystemCode = user.Profile.SystemCode,
                        Permissions = user.Profile.Permissions.Select(pm => new ProfilePermissionDTO()
                        {
                            ID = pm.Id,
                            ModuleId = pm.ModuleId,
                            Write = pm.Write,
                            Read = pm.Read,
                            Remove = pm.Remove,
                            Modify = pm.Modify,
                            Type = (int)pm.Type
                        }),
                        SmtpSettings = user.Profile.SmtpSettings
                    },
                    role = new RoleInfo()
                    {
                        RoleId = user.Role.Id,
                        UserId = user.Id
                    },
                    groups = user.Groups.Select(x => x.UserGroupId).ToList()
                }).SingleOrDefaultAsync();

            //return account object.
            return userInfo;
        }

        public async Task<int> UpdateAsync(TenantUser user)
        {
            return await DbContext.SaveChangesAsync();
            //if (result > 0 && !string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
            //{
            //    if (_warehouse != null && _warehouse.DatabaseName != "0")
            //        BackgroundJob.Enqueue(() => _warehouse.UpdateTenantUser(user.Id, _warehouse.DatabaseName, CurrentUser));
            //}
        }

        /// <summary>
        /// Gets a user by its id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public TenantUser GetById(int userId)
        {
            return DbContext.Users
                .Include(x => x.Profile)
                .FirstOrDefault(x => x.Id == userId);
        }

        public TenantUser GetByIdSync(int userId)
        {
            return DbContext.Users
                .Include(x => x.Profile)
                .FirstOrDefault(x => x.Id == userId);
        }

        public async Task<TenantUser> GetByIdWithPermission(int userId)
        {
            return await DbContext.Users
            .Include(x => x.Profile)
            .ThenInclude(x => x.Permissions)
            .Include(x => x.Groups)
            .FirstOrDefaultAsync(x => x.Id == userId && x.IsActive && !x.Deleted);
        }

        /// <summary>
        /// Gets a user by its email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<TenantUser> GetByEmail(string email)
        {
            return await DbContext.Users.FirstOrDefaultAsync(x => x.Email == email && x.IsActive);
        }

        /// <summary>
        /// Gets users by an integer id list.
        /// </summary>
        /// <param name="userIds"></param>
        /// <returns></returns>
        public async Task<ICollection<TenantUser>> GetByIds(List<int> userIds)
        {
            return await DbContext.Users
                .Where(x => !x.Deleted && userIds.Contains(x.Id))
                .ToListAsync();
        }

        // Gets users by an profile id list.
        public async Task<ICollection<TenantUser>> GetByProfileIds(List<int> profileIds)
        {
            return await DbContext.Users
                .Where(x => !x.Deleted && profileIds.Contains(x.Profile.Id))
                .ToListAsync();
        }

        /// <summary>
        /// Gets all users excluding the subscriber.
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<TenantUser>> GetNonSubscribers()
        {
            return await DbContext.Users.Where(x => !x.Deleted && !x.IsSubscriber).ToListAsync();
        }

        public Task<int> GetTenantUserCount()
        {
            return DbContext.Users.Where(x => x.Deleted == false && x.IsActive).CountAsync();
        }

        /// <summary>
        /// Deletes given user and all related entities from tenant database.
        /// Warning: Do not use this metod for standard tenants. It is written only for demo users and specific needs. 
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task<int> TerminateUser(TenantUser user)
        {
            var roles = await DbContext.Roles.Where(x => x.CreatedById == user.Id).ToListAsync();
            var profiles = await DbContext.Profiles.Where(x => x.CreatedById == user.Id).ToListAsync();
            var profilePermissions = await DbContext.ProfilePermissions.Where(x => x.Module.CreatedById == user.Id).ToListAsync();
            var auditLogs = await DbContext.AuditLogs.Where(x => x.CreatedById == user.Id).ToListAsync();
            var updatedRoles = await DbContext.Roles.Where(x => x.UpdatedById == user.Id).ToListAsync();

            foreach (Role role in updatedRoles)
            {
                role.UpdatedById = null;
            }

            DbContext.Roles.RemoveRange(roles);
            DbContext.ProfilePermissions.RemoveRange(profilePermissions);
            DbContext.Profiles.RemoveRange(profiles);
            DbContext.AuditLogs.RemoveRange(auditLogs);
            DbContext.Users.Remove(user);
            return await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Studio Repository
        /// </summary>
        /// <param name="paginationModel"></param>
        /// <returns></returns>
        public async Task<int> Count()
        {
            return await DbContext.Users
                .Where(x => !x.Deleted & x.Id != 1 && x.IsActive)
                .CountAsync();
        }

        public IQueryable<TenantUser> Find()
        {
            /* Studio user can manage user active status in panel.
             * Because of this we also need to get inactive users.
             */
            var users = DbContext.Users
                .Include(x => x.Profile)
                .Include(x => x.Role)
                .Where(x => !x.Deleted && x.Id != 1/*&& x.IsActive*/)
                .OrderByDescending(x => x.Id);

            return users;
        }

        public TenantUser GetSubscriber()
        {
            return DbContext.Users.Where(x => !x.Deleted && x.IsActive && x.IsSubscriber).FirstOrDefault();
        }
    }
}