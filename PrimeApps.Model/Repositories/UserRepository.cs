using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Context;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Common.Role;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
    public class UserRepository : RepositoryBaseTenant, IUserRepository
    {
        private Warehouse _warehouse;
        private IConfiguration _configuration;

        public UserRepository(TenantDBContext dbContext) : base(dbContext) { }

        public UserRepository(TenantDBContext dbContext, Warehouse warehouse, IConfiguration configuration) : base(dbContext)
        {
            _warehouse = warehouse;
            _configuration = configuration;
        }
        /// <summary>
        /// Creates a new user.
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public async Task CreateAsync(TenantUser user)
        {
            DbContext.Users.Add(user);

            var result = await DbContext.SaveChangesAsync();
            if (result > 0 && !string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
            {
                var platformTenantId = int.Parse(_configuration.GetSection("AppSettings")["PrimeAppsTenantId"]);

                if (_warehouse.DatabaseName != "0")
                    BackgroundJob.Enqueue(() => _warehouse.CreateTenantUser(user.Id, _warehouse.DatabaseName, CurrentUser.TenantId, user.Culture.Contains("tr") ? "tr" : "en", platformTenantId));
            }

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
        public async Task<UserInfo> GetUserInfoAsync(int userId)
        {
            var userInfo = await DbContext.Users.Where(x => x.Id == userId && x.IsActive == true)
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
                               ID = user.Id,
                               currency = user.Currency,
                               createdAt = user.CreatedAt,
                               profile = new ProfileDTO()
                               {
                                   ID = user.Profile.Id,
                                   Description = user.Profile.Description,
                                   Name = user.Profile.Name,
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
                                   Permissions = user.Profile.Permissions.Select(pm => new ProfilePermissionDTO()
                                   {
                                       ID = pm.Id,
                                       ModuleId = pm.ModuleId,
                                       Write = pm.Write,
                                       Read = pm.Read,
                                       Remove = pm.Remove,
                                       Modify = pm.Modify,
                                       Type = (int)pm.Type
                                   })
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

        public async Task UpdateAsync(TenantUser user)
        {
            var result = await DbContext.SaveChangesAsync();
            if (result > 0 && !string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
            {
                var platformTenantId = int.Parse(_configuration.GetSection("AppSettings")["PrimeAppsTenantId"]);

                if (_warehouse.DatabaseName != "0")
                    BackgroundJob.Enqueue(() => _warehouse.UpdateTenantUser(user.Id, _warehouse.DatabaseName, CurrentUser.TenantId, platformTenantId));
            }
        }

        /// <summary>
        /// Gets a user by its id.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<TenantUser> GetById(int userId)
        {
            return await DbContext.Users.FindAsync(userId);
        }

        public TenantUser GetByIdSync(int userId)
        {
            return DbContext.Users
                .Include(x => x.Profile)
                .FirstOrDefault(x => x.Id == userId);
        }

        /// <summary>
        /// Gets a user by its email.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<TenantUser> GetByEmail(string email)
        {
            return await DbContext.Users.FirstOrDefaultAsync(x => x.Email == email);
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

    }
}
