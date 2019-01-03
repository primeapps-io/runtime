using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories
{
    public class PlatformUserRepository : RepositoryBasePlatform, IPlatformUserRepository
    {
        public PlatformUserRepository(PlatformDBContext dbContext, IConfiguration configuration, ICacheHelper cacheHelper) : base(dbContext, configuration, cacheHelper)
        {
        }

        public async Task<PlatformUser> Get(int platformUserId)
        {
            return await DbContext.Users
                .Include(x => x.TenantsAsUser)
                .Where(x => x.Id == platformUserId)
                .SingleOrDefaultAsync();
        }

        public async Task<PlatformUser> Get(string email)
        {
            return await DbContext.Users.Where(x => x.Email == email).SingleOrDefaultAsync();
        }

        public async Task<List<PlatformUser>> GetByIds(List<int> ids)
        {
            return await DbContext.Users.Where(x => ids.Contains(x.Id)).ToListAsync();
        }

        public async Task<PlatformUser> GetSettings(int platformUserId)
        {
            return await DbContext.Users
                .Include(x => x.Setting)
                .Where(x => x.Id == platformUserId)
                .SingleOrDefaultAsync();
        }

        public async Task<PlatformUser> GetWithTenant(int platformUserId, int tenantId)
        {

            var user = await DbContext.Users
                .Include(x => x.Setting)
                .Include(x => x.TenantsAsUser).ThenInclude(y => (y as UserTenant).Tenant).ThenInclude(z => z.Setting)
                .Where(x => x.Id == platformUserId)
                .SingleOrDefaultAsync();

            user.TenantsAsUser = user.TenantsAsUser.Where(x => x.TenantId == tenantId).ToList();

            return user;
        }

        public async Task UpdateAsync(PlatformUser userToEdit)
        {
            await DbContext.SaveChangesAsync();
        }

        public async Task<PlatformUser> GetUserByAutoId(int autoId)
        {
            return await DbContext.Users.Where(x => x.Id == autoId).SingleOrDefaultAsync();
        }

        public async Task<int> GetIdByEmail(string email)
        {
            return await DbContext.Users.Where(x => x.Email == email).Select(x => x.Id).SingleOrDefaultAsync();
        }
        public async Task<PlatformUser> GetWithSettings(string email)
        {
            return await DbContext.Users
                .Include(x => x.Setting)
                .Include(x => x.TenantsAsUser)
                    .ThenInclude(y => (y as UserTenant).Tenant)
                .Where(x => x.Email == email)
                .SingleOrDefaultAsync();
        }

        public async Task<PlatformUser> GetWithTenants(string email)
        {
            var platformKey = typeof(PlatformUser).Name + "_" + email;
            var tenantKey = "tenant_user_";

            var cachedPlatformUser = await CacheHelper.GetAsync<PlatformUser>(platformKey);

            if (cachedPlatformUser != null)
            {
                tenantKey += cachedPlatformUser.Id;
                var cachedTenant = await CacheHelper.GetAsync<Tenant>(tenantKey);

                if (cachedTenant != null)
                {
                    var tenantsAsUserCached = new List<UserTenant>();
                    tenantsAsUserCached.Add(new UserTenant { Tenant = cachedTenant, TenantId = cachedTenant.Id });
                    cachedPlatformUser.TenantsAsUser = tenantsAsUserCached;

                    return cachedPlatformUser;
                }
            }

            var user = await DbContext.Users
                .Include(x => x.Setting)
                .Include(x => x.TenantsAsUser)
                    .ThenInclude(y => (y as UserTenant).Tenant)
                .Include(x => x.TenantsAsOwner)
                .Where(x => x.Email == email)
                .SingleOrDefaultAsync();

            var tenantForCache = user.TenantsAsUser.First().Tenant;
            await CacheHelper.SetAsync(tenantKey + user.Id, tenantForCache);

            var tempUserForCache = user;
            tempUserForCache.TenantsAsOwner = null;
            tempUserForCache.TenantsAsUser = null;

            await CacheHelper.SetAsync(platformKey, tempUserForCache);

            return user;
        }

        public async Task<Tenant> GetTenantWithOwner(int tenantId)
        {
            return await DbContext.Tenants
                .Include(x => x.Owner)
                    .ThenInclude(x => x.Setting)
                .Where(x => x.Id == tenantId)
                .SingleOrDefaultAsync();
        }

        public async Task<int> CreateUser(PlatformUser user)
        {
            DbContext.Users.Add(user);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> CreateTenant(Tenant tenant)
        {
            DbContext.Tenants.Add(tenant);
            return await DbContext.SaveChangesAsync();
        }

        public PlatformUser GetByEmailAndTenantId(string email, int tenantId)
        {
            //var platformKey = "platform_user_" + email;
            //var tenantKey = "tenant_user_";
            var user = new UserTenant();

            //var cachedTenant = CacheHelper.Get<Tenant>(tenantKey);

            //if (cachedTenant != null)
            //{

            //}

            user = DbContext.UserTenants
               .Include(x => x.Tenant)
               .Include(x => x.Tenant).ThenInclude(z => z.Setting)
               .Include(x => x.Tenant).ThenInclude(z => z.License)
               .Include(x => x.Tenant).ThenInclude(z => z.App).ThenInclude(z => z.Setting)
               .Include(x => x.PlatformUser).ThenInclude(z => z.Setting)
               .SingleOrDefault(x => x.PlatformUser.Email == email && x.TenantId == tenantId);

            return user?.PlatformUser;
        }

        public async Task<int> DeleteAsync(PlatformUser user)
        {
            DbContext.Users.Remove(user);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<Tenant> GetTenantByEmailAndAppId(string email, int appId)
        {
            var userTenant = await DbContext.UserTenants
                .Include(x => x.PlatformUser)
                .Include(x => x.Tenant).ThenInclude(z => z.App)
                /*.Include(x => x.TenantsAsUser).ThenInclude(z => z.Setting)
				.Include(x => x.TenantsAsUser).ThenInclude(z => z.License)
				.Include(x => x.TenantsAsUser).ThenInclude(z => z.App).ThenInclude(z => z.Setting)*/
                .SingleOrDefaultAsync(x => x.PlatformUser.Email == email && x.Tenant.AppId == appId);

            return userTenant?.Tenant;
        }

        /// <summary>
        /// Checks if that email address is available.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<EmailAvailableType> IsEmailAvailable(string email, int appId)
        {
            var user = await DbContext.Users
                .Include(x => x.TenantsAsUser)
                    .ThenInclude(y => (y as UserTenant).Tenant)
                .Include(x => x.TenantsAsOwner)
                .Where(x => x.Email == email)
                .SingleOrDefaultAsync();

            if (user != null)
            {
                var appTenant = user.TenantsAsUser.FirstOrDefault(x => x.Tenant.AppId == appId);

                return appTenant != null ? EmailAvailableType.NotAvailable : EmailAvailableType.AvailableForApp;
            }

            //return status.
            return EmailAvailableType.Available;
        }

        public async Task<List<PlatformUser>> GetAllByTenant(int tenantId)
        {
            var tenant = await DbContext.Tenants
                 .Include(x => x.TenantUsers)
                 .Include(x => x.TenantUsers.Select(z => z.PlatformUser))
                 .FirstOrDefaultAsync(x => x.Id == tenantId);

            return tenant?.TenantUsers.Select(x => x.PlatformUser).ToList();
        }

        public async Task<string> GetEmail(int userId)
        {
            return await DbContext.Users.Where(x => x.Id == userId).Select(x => x.Email).SingleOrDefaultAsync();
        }

        public async Task<IList<Workgroup>> MyWorkgroups(int id)
        {
            //create result lists.
            IList<Workgroup> result = null;

            //get instances by user id, then fetch entity types with it's fields.
            result = await DbContext.Tenants
                .Where(x => x.OwnerId == id)
                .Select(i => new Workgroup //create workgroup dto and assign its fields.
                {
                    TenantId = i.Id,
                    Title = i.Title,
                    OwnerId = i.OwnerId,
                    Users = i.TenantUsers.Select(u => new UserList //get users for the instance.
                    {
                        Id = u.PlatformUser.Id,
                        userName = u.PlatformUser.FirstName + " " + u.PlatformUser.LastName,
                        email = u.PlatformUser.Email,
                        hasAccount = true,
                        isAdmin = u.TenantId == u.PlatformUser.Id
                    }).OrderByDescending(x => x.isAdmin).ToList()
                }).ToListAsync();

            //return workgroup object.
            return result;
        }

        public async Task<int> GetTenantModuleLicenseCount(int tenantId)
        {
            var moduleLicenseCount = await DbContext.TenantLicenses.Where(x => x.TenantId == tenantId)
                  .Select(x => x.ModuleLicenseCount).SingleOrDefaultAsync();

            return moduleLicenseCount;
        }

        public PlatformUser GetByEmail(string email)
        {
            return DbContext.Users.Include(x => x.Setting).SingleOrDefault(x => x.Email == email);
        }
    }
}
