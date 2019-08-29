using PrimeApps.Model.Entities.Platform;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IApplicationRepository : IRepositoryBasePlatform
	{
		Task<App> Get(string domain);
		Task<App> GetWithAuth(string domain);
		Task<App> Get(int? id);
        Task<App> GetByNameAsync(string name);
        App GetByName(string name);
        App GetAppById(int? id);
        Task<int> GetAppIdWithDomain(string domain);
        Task<App> GetAppWithDomain(string domain);
    }
}
