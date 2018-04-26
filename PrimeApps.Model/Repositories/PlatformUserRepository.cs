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
using PrimeApps.Model.Common.User;

namespace PrimeApps.Model.Repositories
{
    public class PlatformUserRepository : RepositoryBasePlatform, IPlatformUserRepository
    {
        public PlatformUserRepository(PlatformDBContext dbContext) : base(dbContext)
        {

        }

        public async Task<PlatformUser> Get(int platformUserId)
        {
            return await DbContext.Users.Where(x => x.Id == platformUserId).SingleOrDefaultAsync();
        }

        public async Task<PlatformUser> GetWithTenant(int platformUserId)
        {
            return await DbContext.Users.Include(x => x.Tenant).Where(x => x.Id == platformUserId).SingleOrDefaultAsync();
        }

        public async Task<IList<PlatformUser>> GetExpiredUsers()
        {
            DateTime fifteenDaysBefore = DateTime.Now.AddDays(-15);
            return await DbContext.Users.Include(x => x.Tenant).Where(x => x.EmailConfirmed && x.CreatedAt < fifteenDaysBefore && !x.Tenant.IsPaidCustomer && !x.Tenant.IsDeactivated).ToListAsync();
        }

        public async Task<IList<PlatformUser>> GetAllAccountOwners()
        {
            return await DbContext.Users.Include(x => x.Tenant).Where(x => !x.Tenant.IsDeactivated && x.TenantId == x.Id).ToListAsync();
        }

        public async Task<IList<int>> GetExpiredTenantIdsToDelete()
        {
            DateTime lastMonth = DateTime.Now.AddMonths(-1);
            return await DbContext.Users.Include(x => x.Tenant).Where(x => x.Tenant.IsDeactivated && !x.Tenant.IsSuspended && x.Id == x.TenantId && x.AppId < 6 && x.Tenant.DeactivatedAt < lastMonth).OrderBy(x => x.TenantId).Select(x => x.TenantId.Value).ToListAsync();

        }

        public async Task<IList<PlatformUser>> GetTrialUsers()
        {
            DateTime minusFourteenDays = DateTime.Now.AddDays(-14),
                minusFourteenDaysPlusOneHours = minusFourteenDays.AddHours(1);

            return await DbContext.Users.Include(x => x.Tenant).Where(x => x.EmailConfirmed && x.CreatedAt > minusFourteenDays && x.CreatedAt <= minusFourteenDaysPlusOneHours && x.Id == x.TenantId && !x.Tenant.IsPaidCustomer).ToListAsync();
        }

        /// <summary>
        /// Gets avatar full url
        /// </summary>
        /// <returns></returns>
        public static string GetAvatarUrl(string avatar)
        {
            if (string.IsNullOrWhiteSpace(avatar))
                return string.Empty;

            var blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");

            return $"{blobUrl}/user-images/{avatar}";
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

        /// <summary>
        /// Checks if that email address is available.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> IsEmailAvailable(string email)
        {
            bool status = true;

            //get session and check the email address
            var result = await DbContext.Users.Where(x => x.Email == email || x.ActiveDirectoryEmail == email).SingleOrDefaultAsync();

            if (result != null)
            {
                //the email address exists so set the variable to false.
                status = false;
            }

            //return status.
            return status;
        }

        /// <summary>
        /// Checks if that active directory email address is available.
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public async Task<bool> IsActiveDirectoryEmailAvailable(string email)
        {
            bool status = true;

            //get session and check the email address
            var result = await DbContext.Users.Where(x => x.ActiveDirectoryEmail == email).SingleOrDefaultAsync();

            if (result != null)
            {
                //the email address exists so set the variable to false.
                status = false;
            }

            //return status.
            return status;
        }

        public async Task<PlatformUser> Get(string email)
        {
            return await DbContext.Users.Where(x => x.Email == email).SingleOrDefaultAsync();
        }

        public async Task<List<PlatformUser>> GetAllByTenant(int tenantId)
        {
            return await DbContext.Users.Where(x => x.TenantId == tenantId).ToListAsync();
        }

        public async Task<ActiveDirectoryTenant> GetConfirmedActiveDirectoryTenant(int tenantId)
        {
            return await DbContext.ActiveDirectoryTenants.FirstOrDefaultAsync(x => x.TenantId == tenantId && x.Confirmed);
        }

        public async Task<PlatformUser> GetUserByActiveDirectoryTenantEmail(string email)
        {
            return await DbContext.Users.Where(x => x.ActiveDirectoryEmail == email).SingleOrDefaultAsync();

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
                    Users = i.Users.Select(u => new UserList //get users for the instance.
                    {
                        Id = u.Id,
                        userName = u.FirstName + " " + u.LastName,
                        email = u.Email,
                        hasAccount = true,
                        isAdmin = u.TenantId == u.Id,
                        activeDirectoryEmail = u.ActiveDirectoryEmail
                    }).OrderByDescending(x => x.isAdmin).ToList()
                }).ToListAsync();

            //return workgroup object.
            return result;
        }

        public PlatformUser GetByEmailAndTenantId(string email, int tenantId)
        {
            var user = DbContext.Users
                .Include(x => x.TenantsAsUser.Where(z => z.Id == tenantId))
                .Include(x => x.TenantsAsUser.Select(z => z.Setting))
                .Include(x => x.TenantsAsUser.Select(z => z.License))
                .SingleOrDefault(x => x.Email == email);

            return user;
        }
    }
}
