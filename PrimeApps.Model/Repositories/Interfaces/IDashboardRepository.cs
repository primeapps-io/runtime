using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Application;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IDashboardRepository : IRepositoryBaseTenant
	{
        Task<ICollection<Dashboard>> GetAllBasic(UserItem appUser);
        Task<int> Create(Dashboard dashboard);
        Task<ICollection<Chart>> GetAllChart();
        Task<ICollection<Widget>> GetAllWidget();
        Task<Widget> GetWidgetByViewId(int id);

    }
}
