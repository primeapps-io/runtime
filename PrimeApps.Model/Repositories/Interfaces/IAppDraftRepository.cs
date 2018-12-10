using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IAppDraftRepository : IRepositoryBaseConsole
    {
        Task<List<AppDraft>> GetByOrganizationId(int organizationId);
        Task<List<AppDraft>> GetAll(int userId, string search = "", int page = 0);
    }
}
