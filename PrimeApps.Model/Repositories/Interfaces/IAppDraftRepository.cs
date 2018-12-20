using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IAppDraftRepository : IRepositoryBaseConsole
    {
        Task<AppDraft> Get(string name);
        Task<AppDraft> Get(int id);
        Task<int> Create(AppDraft app);
        Task<int> Update(AppDraft app);
        Task<int> Delete(AppDraft app);
        Task<List<int>> GetByTeamId(int id);
        Task<List<AppDraft>> GetByOrganizationId(int userId, int organizationId);
        Task<List<AppDraft>> GetAllByUserId(int userId, string search = "", int page = 0, AppDraftStatus status = AppDraftStatus.NotSet);
    }
}
