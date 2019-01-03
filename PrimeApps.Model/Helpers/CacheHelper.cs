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
        Task<T> GetAsync<T>(string key);
        T Get<T>(string key);
        Task SetAsync(string key, object data);
        void Set(string key, object data);
        Task<bool> RemoveAsync(string key);
        bool Remove(string key);
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

        public async Task<T> GetAsync<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new NullReferenceException("Key of Cache is not empty or null");

            var result = await _cacheService.GetStringAsync(key);

            if (string.IsNullOrEmpty(result))
                return default(T);

            return JsonConvert.DeserializeObject<T>(result);
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new NullReferenceException("Key of Cache is not empty or null");

            var result = _cacheService.GetString(key);

            if (string.IsNullOrEmpty(result))
                return default(T);

            return JsonConvert.DeserializeObject<T>(result);
        }

        public async Task SetAsync(string key, object data)
        {
            if (string.IsNullOrEmpty(key))
                throw new NullReferenceException("Key of Cache is not empty or null");

            var newData = JsonConvert.SerializeObject(data, Formatting.Indented, CacheSerializerSettings);

            if (string.IsNullOrEmpty(newData))
                throw new JsonSerializationException("Object cannot convert json data for key:" + key);

            await _cacheService.SetStringAsync(key, newData);
        }

        public void Set(string key, object data)
        {
            if (string.IsNullOrEmpty(key))
                throw new NullReferenceException("Key of Cache is not empty or null");

            var newData = JsonConvert.SerializeObject(data, Formatting.Indented, CacheSerializerSettings);

            if (string.IsNullOrEmpty(newData))
                throw new JsonSerializationException("Object cannot convert json data for key:" + key);

            _cacheService.SetString(key, newData);
        }

        public async Task<bool> RemoveAsync(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new NullReferenceException("Key of Cache is not empty or null");

            await _cacheService.RemoveAsync(key);

            return true;
        }

        public bool Remove(string key)
        {
            if (string.IsNullOrEmpty(key))
                throw new NullReferenceException("Key of Cache is not empty or null");

            _cacheService.RemoveAsync(key);

            return true;
        }

    }
}
