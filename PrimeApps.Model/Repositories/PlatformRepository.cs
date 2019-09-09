using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Linq;
using PrimeApps.Model.Entities.Platform;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class PlatformRepository : RepositoryBasePlatform, IPlatformRepository
    {
        //private ICacheHelper _cacheHelper;

        public PlatformRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
            //_cacheHelper = cacheHelper;
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

        public async Task<List<AppTemplate>> GetAppTemplate(int appId, AppTemplateType type, string language, string systemCode = null)
        {
            var template = DbContext.AppTemplates
                .Where(x => x.AppId == appId && x.Language == language && x.Type == type && x.Active);

            if (!string.IsNullOrWhiteSpace(systemCode))
                template = template.Where(x => x.SystemCode == systemCode);

            return await template.ToListAsync();
        }

        public App AppGetById(int id, int userId)
        {
            var app = DbContext.Apps.FirstOrDefault(x => !x.Deleted && x.Id == id);

            //return app;
            return app;
        }

        public List<App> AppGetAll(int userId)
        {
            //var note = await DbContext.Apps
            //    .Where(x => !x.Deleted && x.UserId == userId)
            //    .ToListAsync();

            //return note;
            return null;
        }

        public int AppCreate(App app)
        {
            //app.UserId = CurrentUser.UserId;
            //DbContext.Apps.Add(app);

            //return await DbContext.SaveChangesAsync();
            return 0;
        }

        public int AppUpdate(App app)
        {
            //app.UserId = CurrentUser.UserId;
            //return await DbContext.SaveChangesAsync();
            return 0;
        }

        public int AppDeleteSoft(App app)
        {
            //app.Deleted = true;

            //return await DbContext.SaveChangesAsync();
            return 0;
        }

        public int AppDeleteHard(App app)
        {
            //DbContext.Apps.Remove(app);

            //return await DbContext.SaveChangesAsync();
            return 0;
        }

        public int Count(int appId)
        {
            //Only show Email templates
            var count = DbContext.AppTemplates
               .Where(x => !x.Deleted && x.Type == AppTemplateType.Email && x.AppId == appId).Count();

            return count;
        }

        public async Task<ICollection<AppTemplate>> Find(PaginationModel paginationModel, int? appId)
        {
            var templates = DbContext.AppTemplates
                .Where(x => !x.Deleted && x.Type == AppTemplateType.Email && x.AppId == appId)
                .OrderByDescending(x => x.Id) //&& x.Active
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Module).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    templates = templates.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    templates = templates.OrderByDescending(x => propertyInfo.GetValue(x, null));
                }

            }

            return await templates.ToListAsync();
        }

        public async Task<AppTemplate> GetAppTemplateById(int id)
        {
            var template = await DbContext.AppTemplates.FirstOrDefaultAsync(x => x.Id == id);

            return template;
        }

        public async Task<int> UpdateAppTemplate(AppTemplate template)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> CreateAppTemplate(AppTemplate template)
        {
            DbContext.AppTemplates.Add(template);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<App> AppGetByName(string appName)
        {
            var app = DbContext.Apps.FirstOrDefaultAsync(x => !x.Deleted && x.Name == appName);

            return await app;
        }
        public AppTemplate GetTemplateBySystemCode(int appId, string systemCode, string language)
        {
            var template = DbContext.AppTemplates.FirstOrDefault(x => x.AppId == appId && x.Language == language && x.SystemCode == systemCode && x.Active);

            return template;
        }

        public AppSetting GetAppSettings(int appId)
        {
            var appSettings = DbContext.AppSettings.Where(x => x.AppId == appId).SingleOrDefault();

            return appSettings;
        }

    }
}
