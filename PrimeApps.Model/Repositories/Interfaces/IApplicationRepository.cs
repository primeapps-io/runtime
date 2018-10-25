using PrimeApps.Model.Entities.Platform;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IApplicationRepository : IRepositoryBasePlatform
	{
		Task<App> Get(string domain);
		Task<App> GetWithAuth(string domain);
		Task<App> Get(int? id);
        Task<App> GetByName(string name);
        Task<TeamApp> Get(string organizationCode, string appCode);
		Task<int> GetAppIdWithDomain(string domain);
        Task<App> GetAppWithDomain(string domain);

    }
}
