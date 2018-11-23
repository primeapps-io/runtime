using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    interface ICacheRepository
    {
        Task<T> Get<T>(string key);
        Task<bool> Remove(string key);
        Task<bool> Set(string key, object data);
    }
}