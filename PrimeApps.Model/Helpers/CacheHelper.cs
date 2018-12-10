using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Helpers
{
    public interface ICacheHelper
    {
        Task<T> Get<T>(string key);
        Task<bool> Set(string key, object data);
        Task<bool> Remove(string key);
    }

    public class CacheHelper : ICacheHelper
    {
        private IDistributedCache _cacheService;

        public CacheHelper(IDistributedCache cacheService)
        {
            _cacheService = cacheService;
        }

        public JsonSerializerSettings CacheSerializerSettings => new JsonSerializerSettings
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        };

        public async Task<T> Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return default(T);

            var result = await _cacheService.GetStringAsync(key);

            if (string.IsNullOrEmpty(result))
                return default(T);

            return JsonConvert.DeserializeObject<T>(result);
        }

        public async Task<bool> Set(string key, object data)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            var newData = JsonConvert.SerializeObject(data, Formatting.Indented, CacheSerializerSettings);

            if (string.IsNullOrEmpty(newData))
                return false;

            await _cacheService.SetStringAsync(key, newData);

            return true;
        }

        public async Task<bool> Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;

            await _cacheService.RemoveAsync(key);

            return true;
        }
        
    }
}
