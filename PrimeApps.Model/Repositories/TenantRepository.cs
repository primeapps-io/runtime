using System;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Repositories
{
    public class TenantRepository : RepositoryBasePlatform, ITenantRepository
    {
        public TenantRepository(PlatformDBContext dbContext) : base(dbContext)
        {

        }
        public async Task<Tenant> GetAsync(int tenantId)
        {
            return await DbContext.Tenants.Where(x => x.Id == tenantId).SingleOrDefaultAsync();
        }

		public async Task<Tenant> GetWithSettingsAsync(int tenantId)
		{
			return await DbContext.Tenants
				.Include(x => x.Setting)
				.Where(x => x.Id == tenantId)
				.SingleOrDefaultAsync();
		}

		public async Task<Tenant> GetWithAllAsync(int tenantId)
		{
			return await DbContext.Tenants
				.Include(x => x.Setting)
				.Include(x => x.App)
				.Include(x => x.App.Setting)
				.Where(x => x.Id == tenantId)
				.SingleOrDefaultAsync();
		}

		public async Task<Tenant> GetWithOwnerAsync(int tenantId)
        {
            return await DbContext.Tenants.Include(x => x.Owner).Where(x => x.Id == tenantId).SingleOrDefaultAsync();
        }

        public Tenant Get(int tenantId)
        {
            return DbContext.Tenants.SingleOrDefault(x => x.Id == tenantId);
        }

        public async Task<IList<Tenant>> GetAllActive()
        {
            return await DbContext.Tenants.Include(x => x.License).Where(x => !x.License.IsDeactivated).ToListAsync();
        }

        public async Task UpdateAsync(Tenant tenant)
        {
            await DbContext.SaveChangesAsync();
        }

        public async Task<IList<TenantInfo>> GetTenantInfo(int tenantId)
        {
            return await DbContext.Tenants.Where(x => x.Owner.Id == tenantId)
                 .Select(t => new TenantInfo() //get instances of this user.
                 {
                     tenantId = t.Id,
                     title = t.Title,
                     currency = t.Setting.Currency,
                     language = t.Setting.Language,
                     logo = t.Setting.Logo,
                     logoUrl = t.Setting.Logo,
                     //TODO Removed
                     //hasSampleData = t.HasSampleData,
                     //hasPhone = t.HasPhone,
                     hasAnalytics = t.License.AnalyticsLicenseCount > 0,
                     owner = t.Owner.Id,
                     users = t.TenantUsers.Select(u => new UserList //get users for the instance.
                     {
                         Id = u.UserId,
                         userName = u.PlatformUser.FirstName + " " + u.PlatformUser.LastName,
                         email = u.PlatformUser.Email,
                         hasAccount = true,
                         isAdmin = t.Owner.Id == u.UserId
                     }).OrderByDescending(x => x.isAdmin).ToList()
                 }).ToListAsync();
        }

        public static string GetLogoUrl(string logo)
        {
            if (string.IsNullOrWhiteSpace(logo))
                return string.Empty;

            var blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");

            return $"{blobUrl}/company-logo/{logo}";
        }

        public async Task<Entities.Platform.Tenant> GetByCustomDomain(string customDomain)
        {
            return await DbContext.Tenants.Where(x => x.Setting.CustomDomain == customDomain).SingleOrDefaultAsync();
        }

        public async Task<IList<Tenant>> GetExpiredTenants()
        {
            var fifteenDaysBefore = DateTime.Now.AddDays(-15);

            return await DbContext.Tenants.Include(x => x.License).Where(x => x.CreatedAt < fifteenDaysBefore && !x.License.IsPaidCustomer && !x.License.IsDeactivated).ToListAsync();
        }

        public async Task<IList<int>> GetExpiredTenantIdsToDelete()
        {
            var lastMonth = DateTime.Now.AddMonths(-1);

            return await DbContext.Tenants.Include(x => x.License).Where(x => x.License.DeactivatedAt < lastMonth && !x.License.IsPaidCustomer && !x.License.IsDeactivated).OrderBy(x => x.Id).Select(x => x.Id).ToListAsync();
        }

        public async Task<IList<Tenant>> GetTrialTenants()
        {
            var minusFourteenDays = DateTime.Now.AddDays(-14);
            var minusFourteenDaysPlusOneHours = minusFourteenDays.AddHours(1);

            return await DbContext.Tenants
                .Include(x => x.License)
                .Include(x => x.TenantUsers.Where(z => z.UserId == x.Id))
                .Include(x => x.TenantUsers.Select(z => z.PlatformUser))
				.Include(x => x.App)
				.Include(x => x.App.Setting)
                .Where(x => x.CreatedAt > minusFourteenDays && x.CreatedAt <= minusFourteenDaysPlusOneHours && !x.License.IsPaidCustomer)
                .ToListAsync();
        }
    }
}
