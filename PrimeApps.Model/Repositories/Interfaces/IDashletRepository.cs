using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Dashlet;
using PrimeApps.Model.Entities.Application;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IDashletRepository : IRepositoryBaseTenant
	{
        Task<ICollection<DashletView>> GetDashboardDashlets(int dashboardId, UserItem appUser, IReportRepository reportRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IViewRepository viewRepository, string locale = "", int timezoneOffset = 180);
        Task<int> Create(Dashlet dashlet);
        Task<Dashlet> GetDashletById(int id);
        Task<int> DeleteSoftDashlet(Dashlet dashlet);
        Task<int> UpdateDashlet(Dashlet dashlet);

    }
}
