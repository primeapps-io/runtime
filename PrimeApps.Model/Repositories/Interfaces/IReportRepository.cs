using Newtonsoft.Json.Linq;
using PrimeApps.Model.Entities.Application;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Cache;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IReportRepository : IRepositoryBaseTenant
	{
        Task<JArray> GetDashletReportData(int reportId, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, UserItem appUser, string locale = "", int timezoneOffset = 180, bool roleBasedEnabled = true, bool showDisplayValue = true);
        Task<JArray> GetDashletViewData(int viewId, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, UserItem appUser, string locale = "", int timezoneOffset = 180, bool roleBasedEnabled = true);
        ICollection<Report> GetAllBasic();
        Task<Report> GetById(int id);
        Task<int> Create(Report report);
        Task<int> Update(Report report, List<int> currentFieldIds, List<int> currentFilterIds, List<int> currentAggregationIds);
        Task<int> DeleteSoft(Report report);
        Task<int> DeleteHard(Report report);
        Task<int> DeleteReportShare(ReportShares report, TenantUser user);
        Task<ReportCategory> GetCategoryById(int id);
        ICollection<ReportCategory> GetCategories(int userId);
        Task<int> CreateCategory(ReportCategory reportCategory);
        Task<int> UpdateCategory(ReportCategory reportCategory);
        Task<int> DeleteSoftCategory(ReportCategory reportCategory);
        Task<int> DeleteHardCategory(ReportCategory reportCategory);
        Task<Chart> GetChartByReportId(int reportId);
        Task<int> CreateChart(Chart chart);
        Task<int> UpdateChart(Chart chart);
        Task<int> DeleteSoftChart(Chart chart);
        Task<int> DeleteHardChart(Chart chart);
        Task<Widget> GetWidgetByReportId(int reportId);
        Task<int> CreateWidget(Widget widget);
        Task<int> UpdateWidget(Widget widget);
        Task<int> DeleteSoftWidget(Widget widget);
        Task<int> DeleteHardWidget(Widget widget);
    }
}
