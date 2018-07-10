using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public interface ICacheRepository : IRepositoryBasePlatform
    {
        Task<int> Add<T>(string key, T data);
        T Get<T>(string key);
        Task<bool> Delete(string key);
    }
}