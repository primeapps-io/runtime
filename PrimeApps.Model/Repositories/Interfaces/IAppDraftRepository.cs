using Newtonsoft.Json.Linq;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IAppDraftRepository : IRepositoryBaseStudio
    {
        List<int> GetAppIdsByOrganizationId(int organizationId);
        Task<AppDraft> Get(string name);
        Task<AppDraft> Get(int id);
        Task<List<AppDraft>> GetUserApps(int userId, int organizationId, string search = "", int page = 0);
        Task<AppDraft> GetWithPackages(int id);
        Task<int> Create(AppDraft app);
        Task<int> Update(AppDraft app);
        Task<int> Delete(AppDraft app);
        Task<List<int>> GetByTeamId(int id);
        Task<List<AppDraft>> GetAllByUserId(int userId, string search = "", int page = 0);
        Task<List<AppCollaborator>> GetAppCollaborators(int appId);
        Task<int> UpdateAuthTheme(int id, JObject model);
        Task<AppDraftSetting> GetAuthTheme(int id);
        Task<int> UpdateAppTheme(int id, JObject model);
        Task<AppDraftSetting> GetAppTheme(int id);
		Task<int> CreateAppTemplate(AppDraftTemplate template);
		Task<int> UpdateAppTemplate(AppDraftTemplate template);
		Task<AppDraft> AppGetByName(string appName);
		Task<ICollection<AppDraftTemplate>> Find(PaginationModel paginationModel, int? appId);
		int Count(int appId);
		Task<AppDraftTemplate> GetAppTemplateById(int id);
	}
}