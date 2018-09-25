using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Chart;
using PrimeApps.Model.Common.Dashlet;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories
{
    public class DashletRepository : RepositoryBaseTenant, IDashletRepository
    {
        public DashletRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        /// <summary>
        /// Getting dashboard specific dashlets for user and user is null for the purpose "shared" dashlets view by system admin.
        /// A Dashlet can contain CHARTS AND WIDGETS. A Chart is based on REPORT'S output or CUSTOM SQL'S output. A Widget can contain several items such as COUNTVIEW etc. See WidgetType Enum for details.
        /// If you need "another" dashlet that contain widget, without changing entity, you can add new WidgetType enum. (model changing can be hard at multitenant database system like ofisim.com)
        /// </summary>
        /// <returns></returns>
        public async Task<ICollection<DashletView>> GetDashboardDashlets(int dashboardId, UserItem appUser, IReportRepository reportRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IViewRepository viewRepository, IConfiguration configuration, string locale = "", int timezoneOffset = 180)
        {
            var dashletViews = new List<DashletView>();

            var dashlets = DbContext.Dashlets
                .Include(x => x.Chart)
                .Include(x => x.Chart.Report)
                .Include(x => x.Chart.Report.Aggregations)
                .Include(x => x.Widget)
                .Where(x => x.DashletArea == DashletArea.Dashboard && x.Deleted == false && x.DashboardId == dashboardId)
                .OrderBy(x => x.Order)
                .ToList();

            foreach (var dashlet in dashlets)
            {
                var dashletView = new DashletView
                {
                    Id = dashlet.Id,
                    Name = dashlet.Name,
                    XTileHeight = dashlet.XTileHeight,
                    YTileLength = dashlet.YTileLength,
                    DashletType = dashlet.DashletType,
                    DataResource = dashlet.DashletType == DashletType.Chart ? dashlet.Chart.ReportId.ToString() : dashlet.Widget.ReportId.ToString(),
                    Order = dashlet.Order
                };

                if (dashlet.Chart != null)
                {
                    var aggregation = dashlet.Chart.Report.Aggregations.FirstOrDefault();
                    var showDisplayValue = dashlet.Chart.ChartType != ChartType.Funnel && dashlet.Chart.ChartType != ChartType.Pyramid && aggregation != null && aggregation.AggregationType != AggregationType.Count;
                    var chartData = await reportRepository.GetDashletReportData(dashlet.Chart.ReportId.Value, recordRepository, moduleRepository, picklistRepository, configuration, appUser, locale, timezoneOffset, showDisplayValue: showDisplayValue);

                    dashletView.ChartItem = new ChartItem
                    {
                        Chart = new ChartView
                        {
                            Id = dashlet.Chart.Id,
                            Caption = dashlet.Chart.Caption,
                            ChartType = dashlet.Chart.ChartType,
                            Subcaption = dashlet.Chart.SubCaption,
                            Theme = dashlet.Chart.Theme,
                            Xaxisname = dashlet.Chart.XaxisName,
                            Yaxisname = dashlet.Chart.YaxisName,
                            ReportId = dashlet.Chart.ReportId.ToString(),
                            ReportModuleId = dashlet.Chart.Report.ModuleId,
                            ReportGroupField = dashlet.Chart.Report.GroupField,
                            ReportAggregationField = aggregation != null ? aggregation.Field : ""
                        },
                        Data = chartData != null && chartData.Count > 0 ? new ChartData { Data = chartData } : null
                    };
                }

                if (dashlet.Widget != null)
                {
                    JArray widgetData = null;

                    if (dashlet.Widget.ReportId.HasValue)
                        widgetData = await reportRepository.GetDashletReportData(dashlet.Widget.ReportId.Value, recordRepository, moduleRepository, picklistRepository, configuration, appUser, locale, timezoneOffset);

                    if (dashlet.Widget.ViewId.HasValue)
                        widgetData = await reportRepository.GetDashletViewData(dashlet.Widget.ViewId.Value, recordRepository, moduleRepository, picklistRepository, configuration, appUser, locale, timezoneOffset);

                    dashletView.Widget = new Model.Common.Widget.WidgetView
                    {
                        Id = dashlet.Widget.Id,
                        Color = dashlet.Widget.Color,
                        Icon = dashlet.Widget.Icon,
                        WidgetType = dashlet.Widget.WidgetType,
                        LoadUrl = dashlet.Widget.LoadUrl,
                        Name = dashlet.Widget.Name,
                        WidgetData = widgetData != null && widgetData.Count > 0 ? widgetData[0] : null,
                        ViewId = dashlet.Widget.ViewId != null ? dashlet.Widget.ViewId.Value : 0,
                        ReportId = dashlet.Widget.ReportId != null ? dashlet.Widget.ReportId.Value : 0
                    };
                }

                dashletViews.Add(dashletView);
            }

            return dashletViews;
        }

        public async Task<int> Create(Dashlet dashlet)
        {
            DbContext.Dashlets.Add(dashlet);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoftDashlet(Dashlet dashlet)
        {
            dashlet.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }
        public async Task<int> UpdateDashlet(Dashlet dashlet)
        {
            return await DbContext.SaveChangesAsync();
        }
        public async Task<Dashlet> GetDashletById(int id)
        {
            var dashlet = await DbContext.Dashlets
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return dashlet;
        }

    }
}
