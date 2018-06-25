using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Entities.Application;
using System;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Cache;

namespace PrimeApps.Model.Repositories
{
    public class DashboardRepository : RepositoryBaseTenant, IDashboardRepository
    {
        public DashboardRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<int> Create(Dashboard dashboard)
        {
            DbContext.Dashboards.Add(dashboard);

            return await DbContext.SaveChangesAsync();
        }

        public IQueryable<Dashboard> GetDashboardQuery(UserItem appUser)
        {
            var dashboards = DbContext.Dashboards.Where(x => !x.Deleted);
            if (!appUser.HasAdminProfile)
            {
                dashboards = dashboards.Where(x => x.SharingType == DashboardSharingType.Everybody
                 || (x.UserId == CurrentUser.UserId && x.SharingType == DashboardSharingType.Me)
                 || (x.ProfileId == appUser.ProfileId && x.SharingType == DashboardSharingType.Profile)
                );
            }
            else
            {
                dashboards = dashboards.Where(x => x.SharingType == DashboardSharingType.Everybody
              || (x.UserId == CurrentUser.UserId && x.SharingType == DashboardSharingType.Me)
              || (x.SharingType == DashboardSharingType.Profile)
             );
            }

            return dashboards;
        }

        public async Task<ICollection<Dashboard>> GetAllBasic(UserItem appUser)
        {
            var dashboards = GetDashboardQuery(appUser);

            return await dashboards.ToListAsync();
        }

        public async Task<ICollection<Chart>> GetAllChart()
        {
            var charts = DbContext.Charts.Where(x => (x.Report != null ? !x.Report.Deleted : !x.Deleted));

            return await charts.ToListAsync();
        }
        public async Task<ICollection<Widget>> GetAllWidget()
        {
            var widgets = DbContext.Widgets.Where(x => (x.Report != null ? !x.Report.Deleted : !x.Deleted) && x.ViewId ==null);

            return await widgets.ToListAsync();
        }

        public async Task<Widget> GetWidgetByViewId(int id)
        {
            var widget = await DbContext.Widgets.Where(x => !x.Deleted && x.ViewId == id).FirstOrDefaultAsync();

            return widget;
        }

    }
}
