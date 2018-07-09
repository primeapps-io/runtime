using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class CacheRepository:RepositoryBasePlatform, ICacheRepository
    {
        public CacheRepository(PlatformDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<int> Add<T>(string key,T data)
        {
            if (string.IsNullOrEmpty(key) || data == null)
                return 0;

            Cache result;
            try
            {
                var cacheObject = JsonConvert.SerializeObject(data, Formatting.Indented, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.All
                });
                result = new Cache { Key = key, Value = cacheObject };
            }
            catch(Exception ex)
            {
                return 0;
            }
        
            DbContext.Cache.Add(result);

            return await DbContext.SaveChangesAsync();
        }

        public T Get<T>(string key)
        {
            if (string.IsNullOrEmpty(key))
                return default(T);

            var result = DbContext.Cache.SingleOrDefault(q => q.Key == key);

            if (result == null)
                return default(T);
 
            try
            {
                var data = JsonConvert.DeserializeObject<T>(result.Value, new JsonSerializerSettings
                {
                    ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
                    PreserveReferencesHandling = PreserveReferencesHandling.Objects,
                    TypeNameHandling = TypeNameHandling.All
                });
                return data != null ? data : default(T); 
            }
            catch (Exception ex)
            {
                return default(T);
            }
            
        }

        public async Task<bool> Delete(string key)
        {
            if (string.IsNullOrEmpty(key))
                return false;
            Cache data = new Cache { Key = key, Value = "" };

            DbContext.Cache.Remove(data);

            var result = await DbContext.SaveChangesAsync();

            return result > 0 ? true : false;
        }

        ////User bilgilerini cache'den okuyucak fonk.
        //public PlatformUser GetUserCache(string key)
        //{
        //    var result = DbContext.Cache.SingleOrDefault(q => q.Key == key).Value;

        //    if (string.IsNullOrEmpty(result))
        //        return null;

        //    try
        //    {
        //        var data = JsonConvert.DeserializeObject<PlatformUser>(result, new JsonSerializerSettings
        //        {
        //            ReferenceLoopHandling = ReferenceLoopHandling.Serialize,
        //            PreserveReferencesHandling = PreserveReferencesHandling.Objects,
        //            TypeNameHandling = TypeNameHandling.All
        //        });
        //        return data != null ? data : null;
        //    }
        //    catch (Exception ex)
        //    {
        //        return null;
        //    }

        //}
    }
}
