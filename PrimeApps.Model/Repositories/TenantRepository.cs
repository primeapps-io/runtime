using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
	public class TenantRepository : RepositoryBasePlatform, ITenantRepository
	{
		public TenantRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
		{
		}

		public async Task<Tenant> GetAsync(int tenantId)
		{
			return await DbContext.Tenants
				.Include(x => x.Setting)
				.Where(x => x.Id == tenantId).SingleOrDefaultAsync();
		}

		public async Task<Tenant> GetWithLicenseAsync(int tenantId)
		{
			return await DbContext.Tenants
				.Include(x => x.License)
				.Where(x => x.Id == tenantId)
				.SingleOrDefaultAsync();
		}

		public async Task<Tenant> GetWithSettingsAsync(int tenantId)
		{
			return await DbContext.Tenants
				.Include(x => x.Setting)
				.Include(x => x.App).ThenInclude(x => x.Setting)
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

		public async Task<int> CreateAsync(Tenant tenant)
		{
			DbContext.Tenants.Add(tenant);
			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> DeleteAsync(Tenant tenant)
		{
			DbContext.Tenants.Remove(tenant);
			return await DbContext.SaveChangesAsync();
		}

		public async Task<IList<TenantInfo>> GetTenantInfo(int tenantId, TenantUser tenantUser)
		{
			int tenantUserId = tenantUser != null ? tenantUser.Id : -1;

			var tenant = await DbContext.Tenants
				.Include(x => x.License)
				.Include(x => x.Setting)
				.Where(x => x.Id == tenantId)
				.Select(t => new TenantInfo()//get instances of this user.
				{
					tenantId = t.Id,
					title = t.Title,
					currency = t.Setting.Currency,
					language = t.Setting.Language,
					logoUrl = t.Setting.Logo,
					hasAnalytics = t.License.AnalyticsLicenseCount > 0,
					owner = t.Owner.Id,
					licenses = t.License,
					setting = t.Setting,
					users = t.TenantUsers.Select(u => new UserList//get users for the instance.
					{
						Id = u.UserId,
						userName = u.PlatformUser.FirstName + " " + u.PlatformUser.LastName,
						email = u.PlatformUser.Email,
						hasAccount = true,
						isAdmin = t.Owner.Id == u.UserId,
						isSubscriber = u.UserId == tenantUserId
					}).ToList()
				}).ToListAsync();

			return tenant;
		}

		public async Task<Tenant> GetByCustomDomain(string customDomain)
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

			var tenants = await DbContext.Tenants
				.Include(x => x.License)
				.Include(x => x.TenantUsers)
				.Include(x => (x.TenantUsers as UserTenant).PlatformUser)
				.Include(x => x.App)
				.Include(x => x.App.Setting)
				.Where(x => x.CreatedAt > minusFourteenDays && x.CreatedAt <= minusFourteenDaysPlusOneHours && !x.License.IsPaidCustomer)
				.ToListAsync();

			foreach (var tenant in tenants)
			{
				tenant.TenantUsers = tenant.TenantUsers.Where(x => x.UserId == tenant.Id).ToList();
			}

			return tenants;
		}

		public async Task<IList<int>> GetByAppId(int appId)
		{
			var tenantIds = await DbContext.Tenants
				.Include(x => x.License)
				.Where(x => !x.License.IsDeactivated && x.AppId == appId)
				.OrderBy(x => x.Id)
				.Select(x => x.Id)
				.ToListAsync();

			return tenantIds;
		}
	}
}