using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using PrimeApps.Model.Entities.Platform;
using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories
{
    public class PlatformRepository : RepositoryBasePlatform, IPlatformRepository
    {
        public PlatformRepository(PlatformDBContext dbContext) : base(dbContext) { }

		public App GetAppInfo(string domain)
		{
			var app = DbContext.Apps.Include(x => x.Setting).SingleOrDefault(x => x.Setting.Domain == domain);

			return app;
		}

		public App GetAppInfoWithAuth(string domain)
		{
			var app = DbContext.Apps.Include(x => x.Setting).SingleOrDefault(x => x.Setting.AuthDomain == domain);

			return app;
		}

		public App GetAppInfo(int id)
		{
			var app = DbContext.Apps.Include(x => x.Setting).SingleOrDefault(x => x.Id == id);

			return app;
		}

		public TeamApp GetAppInfo(string organizationName, string appName)
		{
			var app = DbContext.TeamApps
				.Include(y => y.App).ThenInclude(x => x.Setting)
				.Include(y => y.Team).ThenInclude(x => x.Organization)
				.SingleOrDefault(x => x.Team.Organization.Name == organizationName && x.App.Name == appName);

			return app == null ? null : app;
		}

		public Tenant GetTenant(int tenantId)
		{
			var tenant = DbContext.Tenants
				.Include(x => x.License)
				.Include(x => x.Setting)
				.Include(x => x.TenantUsers)
				.SingleOrDefault(x => x.Id == tenantId);

			return tenant;
		}

		public AppTemplate GetAppTemplate(int appId, AppTemplateType type, string systemCode, string language)
		{
			var template = DbContext.AppTemplates
				.SingleOrDefault(x => x.AppId== appId && x.SystemCode == systemCode && x.Language == language && x.Type == type && x.Active == true);

			return template;
		}
		public async Task<App> AppGetById(int id, int userId)
        {
            //var note = await DbContext.Apps
            //    .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id && x.UserId == userId);

            //return note;
            return null;
        }

        public async Task<List<App>> AppGetAll(int userId)
        {
            //var note = await DbContext.Apps
            //    .Where(x => !x.Deleted && x.UserId == userId)
            //    .ToListAsync();

            //return note;
            return null;
        }

		

		public async Task<int> AppCreate(App app)
        {
            //app.UserId = CurrentUser.UserId;
            //DbContext.Apps.Add(app);

            //return await DbContext.SaveChangesAsync();
            return 0;
        }

        public async Task<int> AppUpdate(App app)
        {
            //app.UserId = CurrentUser.UserId;
            //return await DbContext.SaveChangesAsync();
            return 0;
        }

        public async Task<int> AppDeleteSoft(App app)
        {
            //app.Deleted = true;

            //return await DbContext.SaveChangesAsync();
            return 0;
        }

        public async Task<int> AppDeleteHard(App app)
        {
            //DbContext.Apps.Remove(app);

            //return await DbContext.SaveChangesAsync();
            return 0;
        }
    }
}
