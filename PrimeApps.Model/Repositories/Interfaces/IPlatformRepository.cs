using System.Collections.Generic;
using PrimeApps.Model.Entities.Application;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPlatformRepository : IRepositoryBasePlatform
    {
        Task<App> AppGetById(int id, int userId);
        Task<List<App>> AppGetAll(int userId);
        Task<int> AppCreate(App app);
		Tenant GetTenant(int tenantId);
		AppTemplate GetAppTemplate(int appId, AppTemplateType type, string systemCode, string language);
		Task<int> AppUpdate(App app);
        Task<int> AppDeleteSoft(App app);
        Task<int> AppDeleteHard(App app);
    }
}
