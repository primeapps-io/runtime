using System.Collections.Generic;
using PrimeApps.Model.Entities.Application;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IPlatformRepository : IRepositoryBasePlatform
    {
        Task<App> AppGetById(int id, int userId);
        Task<List<App>> AppGetAll(int userId);
        Task<int> AppCreate(App app);
        Task<int> AppUpdate(App app);
        Task<int> AppDeleteSoft(App app);
        Task<int> AppDeleteHard(App app);
    }
}
