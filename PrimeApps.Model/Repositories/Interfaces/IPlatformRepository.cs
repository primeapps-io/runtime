using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories.Interfaces
{
	public interface IPlatformRepository : IRepositoryBasePlatform
	{
		App AppGetById(int id, int userId);
		Task<List<App>> GetAllApp();
		int AppCreate(App app);
		Tenant GetTenant(int tenantId);
		Task<List<AppTemplate>> GetAppTemplate(int appId, AppTemplateType type, string language, string systemCode);
		Task<int> Update(App app);
		int AppDeleteSoft(App app);
		int AppDeleteHard(App app);
		int Count(int appId);
		Task<ICollection<AppTemplate>> Find(PaginationModel paginationModel, int? appId);
		Task<AppTemplate> GetAppTemplateById(int id);

		Task<int> UpdateAppTemplate(AppTemplate template);

		Task<int> CreateAppTemplate(AppTemplate template);

		Task<App> AppGetByName(string appName);
        AppTemplate GetTemplateBySystemCode(int appId, string systemCode, string language);

        Task<AppSetting> GetAppSettings(int appId);
	}
}
