using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities;
using PrimeApps.Model.Enums;
using PrimeApps.App.Models.ViewModel.Crm.Dashboard;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Application;

namespace PrimeApps.App.Helpers
{
    public static class DashboardHelper
    {
        public static async Task<Dashboard> CreateEntity(DashboardBindingModel dashboardModel, UserItem user)
        {


            var dashboard = new Dashboard
            {
                Name = dashboardModel.Name,
                Description = dashboardModel.Description,
                UserId = null,
                ProfileId = null,
                SharingType = dashboardModel.SharingType
            };

            if (user.HasAdminProfile && dashboardModel.ProfileId != null && dashboardModel.SharingType == DashboardSharingType.Profile)
            {
                dashboard.ProfileId = dashboardModel.ProfileId;
            }

            if (!user.HasAdminProfile && dashboardModel.UserId != null && dashboardModel.SharingType == DashboardSharingType.Me)
            {
                dashboard.UserId = dashboardModel.UserId;
            }

            return dashboard;
        }

        public static async Task UpdateEntity(DashboardBindingModel dashboardModel)
        {
            var dashboard = new Dashboard
            {
                Name = dashboardModel.Name,
                Description = dashboardModel.Description,
                UserId = dashboardModel.UserId,
                ProfileId = dashboardModel.ProfileId,
                SharingType = dashboardModel.SharingType
            };

        }

        public static async Task<Dashlet> CreateEntityDashlet(DashletBindingModel dashletModel, IDashboardRepository _dashboardRepository, IReportRepository _reportRepository)
        {
            if (dashletModel.ViewId.HasValue)
            {
                var widget = await _dashboardRepository.GetWidgetByViewId(dashletModel.ViewId.Value);
                if (widget != null)
                {
                    dashletModel.WidgetId = widget.Id;
                    if (widget.Color != dashletModel.Color && widget.Icon != dashletModel.Icon)
                    {
                        widget.Color = dashletModel.Color != null ? dashletModel.Color : widget.Color;
                        widget.Icon = dashletModel.Icon != null ? dashletModel.Icon : widget.Icon;
                        await _reportRepository.UpdateWidget(widget);
                    }

                }
                else
                {

                    var widgetModel = new Widget
                    {
                        Name = dashletModel.Name,
                        Color = dashletModel.Color,
                        Icon = dashletModel.Icon,
                        WidgetType = WidgetType.SummaryCount,
                        ViewId = dashletModel.ViewId
                    };

                    var createWidet = await _reportRepository.CreateWidget(widgetModel);
                    dashletModel.WidgetId = widgetModel.Id;
                }
            }


            var dashlet = new Dashlet
            {
                Name = dashletModel.Name,
                DashletArea = DashletArea.Dashboard,
                DashletType = dashletModel.DashletType,
                ChartId = dashletModel.ChartId,
                WidgetId = dashletModel.WidgetId,
                Order = dashletModel.Order,
                XTileHeight = dashletModel.XTileHeight,
                YTileLength = dashletModel.YTileLength,
                DashboardId = dashletModel.DashboardId
            };

            return dashlet;
        }

        public static async Task<Dashlet> UpdateEntityDashlet(DashletBindingModel dashletModel, Dashlet dashlet, IDashboardRepository _dashboardRepository, IReportRepository _reportRepository)
        {

            if (dashletModel.ViewId.HasValue)
            {
                var widget = await _dashboardRepository.GetWidgetByViewId(dashletModel.ViewId.Value);
                if (widget != null)
                {
                    dashletModel.WidgetId = widget.Id;
                    widget.ViewId = dashletModel.ViewId.Value;
                    widget.Color = dashletModel.Color != null ? dashletModel.Color : widget.Color;
                    widget.Icon = dashletModel.Icon != null ? dashletModel.Icon : widget.Icon;
                    widget.Name = dashletModel.Name;
                   
                    await _reportRepository.UpdateWidget(widget);
                }
                else
                {

                    var widgetModel = new Widget
                    {   Name= dashletModel.Name,
                        Color = dashletModel.Color,
                        Icon = dashletModel.Icon,
                        WidgetType = WidgetType.SummaryCount,
                        ViewId = dashletModel.ViewId
                    };

                    var createWidet = await _reportRepository.CreateWidget(widgetModel);
                    dashlet.WidgetId = widgetModel.Id;
                }
            }


            dashlet.Name = dashletModel.Name;           
            dashlet.XTileHeight = dashletModel.XTileHeight;
            dashlet.YTileLength = dashletModel.YTileLength;

            return dashlet;
        }
        public static ChartViewModel MapToViewModel(Chart chart)
        {
            var ChartViewModel = new ChartViewModel
            {
                Id = chart.Id,
                Name = chart.Caption
            };

            return ChartViewModel;
        }

        public static List<ChartViewModel> MapToViewModel(ICollection<Chart> charts)
        {
            return charts.Select(MapToViewModel).ToList();
        }

        public static WidgetViewModel MapToWidgetModel(Widget widget)
        {
            var WidgetViewModel = new WidgetViewModel
            {
                Id = widget.Id,
                Name = widget.Name
            };

            return WidgetViewModel;
        }

        public static List<WidgetViewModel> MapToViewModel(ICollection<Widget> widgets)
        {
            return widgets.Select(MapToWidgetModel).ToList();
        }
    }
}