using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Configuration;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Common.User;

namespace PrimeApps.Model.Repositories
{
    public class TenantRepository : RepositoryBasePlatform, ITenantRepository
    {
        public TenantRepository(PlatformDBContext dbContext) : base(dbContext)
        {

        }
        public async Task<Entities.Platform.Tenant> GetAsync(int tenantId)
        {
            return await DbContext.Tenants.Where(x => x.Id == tenantId).SingleOrDefaultAsync();
        }

        public async Task<Entities.Platform.Tenant> GetWithOwnerAsync(int tenantId)
        {
            return await DbContext.Tenants.Include(x=>x.Owner).Where(x => x.Id == tenantId).SingleOrDefaultAsync();
        }

        public Entities.Platform.Tenant Get(int tenantId)
        {
            return DbContext.Tenants.Where(x => x.Id == tenantId).SingleOrDefault();
        }
        public async Task UpdateAsync(Entities.Platform.Tenant tenant)
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
                     currency = t.Currency,
                     language = t.Language,
                     logo = t.Logo,
                     logoUrl = t.Logo,
                     hasSampleData = t.HasSampleData,
                     hasAnalytics = t.HasAnalytics,
                     hasPhone = t.HasPhone,
                     owner = t.Owner.Id,
                     users = t.Users.Select(u => new UserList //get users for the instance.
                     {
                         Id = u.Id,
                         userName = u.FirstName + " " + u.LastName,
                         email = u.Email,
                         hasAccount = true,
                         isAdmin = t.Owner.Id == u.Id
                     }).OrderByDescending(x => x.isAdmin).ToList()
                 }).ToListAsync();
        }

        /// <summary>
        /// Gets logo full url
        /// </summary>
        /// <returns></returns>
        public static string GetLogoUrl(string logo)
        {
            if (string.IsNullOrWhiteSpace(logo))
                return string.Empty;

            var blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");

            return $"{blobUrl}/company-logo/{logo}";
        }

        public async Task<Entities.Platform.Tenant> GetByCustomDomain(string customDomain)
        {
            return await DbContext.Tenants.Where(x => x.CustomDomain == customDomain).SingleOrDefaultAsync();
        }

        public async Task<int> GetUserCount(int tenantId)
        {
            return await DbContext.Users.Where(x => x.TenantId == tenantId).CountAsync();
        }
    }
}
