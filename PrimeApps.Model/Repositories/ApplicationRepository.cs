﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public class ApplicationRepository : RepositoryBasePlatform, IApplicationRepository
    {
        public ApplicationRepository(PlatformDBContext dbContext, IConfiguration configuration, ICacheHelper cacheHelper) : base(dbContext, configuration, cacheHelper) { }
        public async Task<App> Get(string domain)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Setting.AppDomain == domain);
        }
        public async Task<App> Get(int? id)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<TeamApp> Get(string organizationName, string appName)
        {
            var app = await DbContext.TeamApps
                .Include(y => y.App).ThenInclude(x => x.Setting)
                .Include(y => y.Team).ThenInclude(x => x.Organization)
                .FirstOrDefaultAsync(x => x.Team.Organization.Name == organizationName && x.App.Name == appName);

            return app ?? null;
        }

        public async Task<App> GetByName(string name)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Name == name);
        }

        public async Task<App> GetWithAuth(string domain)
        {
            var app = await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Setting.AuthDomain == domain);

            return app;
        }

        public async Task<int> GetAppIdWithDomain(string domain)
        {
            var app = await DbContext.AppSettings
                .FirstOrDefaultAsync(x => x.AppDomain == domain);

            return app == null ? 0 : app.AppId;
        }

        public async Task<App> GetAppWithDomain(string domain)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .FirstOrDefaultAsync(x => x.Setting.AppDomain == domain);
        }

    }
}
