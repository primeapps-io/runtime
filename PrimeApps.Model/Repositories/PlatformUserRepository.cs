using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories
{
    public class PlatformUserRepository : RepositoryBasePlatform, IPlatformUserRepository
    {
        public PlatformUserRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
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
                .Where(x => x.Email == email)
                .SingleOrDefaultAsync();
        }

        public async Task<PlatformUser> GetWithTenants(string email)
        {
            return await DbContext.Users
                .Include(x => x.Setting)
                .Include(x => x.TenantsAsUser)
                .Include(x => x.TenantsAsOwner)
                .Where(x => x.Email == email)
                .SingleOrDefaultAsync();
        }

        public async Task<Tenant> GetTenantWithOwner(int tenantId)
        {
            return await DbContext.Tenants
                .Include(x => x.Owner).ThenInclude(x => x.Setting)
                .Where(x => x.Id == tenantId).SingleOrDefaultAsync();
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
            var user = DbContext.UserTenants
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
                .SingleOrDefaultAsync(x => x.PlatformUser.Email == email && x.Tenant.AppId == appId);

            return userTenant?.Tenant;
        }

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
            var result = await DbContext.Tenants
                .Where(x => x.OwnerId == id)
                .Select(i => new Workgroup
                {
                    TenantId = i.Id,
                    Title = i.Title,
                    OwnerId = i.OwnerId,
                    Users = i.TenantUsers.Select(u => new UserList//get users for the instance.
                    {
                        Id = u.PlatformUser.Id,
                        userName = u.PlatformUser.FirstName + " " + u.PlatformUser.LastName,
                        email = u.PlatformUser.Email,
                        hasAccount = true,
                        isAdmin = u.TenantId == u.PlatformUser.Id
                    }).OrderByDescending(x => x.isAdmin).ToList()
                }).ToListAsync();

            return result;
        }

        public async Task<int> GetTenantModuleLicenseCount(int tenantId)
        {
            var moduleLicenseCount = await DbContext.TenantLicenses.Where(x => x.TenantId == tenantId)
                .Select(x => x.ModuleLicenseCount).SingleOrDefaultAsync();

            return moduleLicenseCount;
        }
    }
}