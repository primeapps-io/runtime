using Microsoft.IdentityModel.Clients.ActiveDirectory;
using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Context;
using Microsoft.EntityFrameworkCore;

namespace PrimeApps.App.Providers
{
	//TODO Removed
    /*public class AdTokenCache : TokenCache
    {
        private string _user;
        private ActiveDirectoryCache _cache;

        // constructor
        public AdTokenCache(string user)
        {
            // associate the cache to the current user of the web app
            _user = user;

            AfterAccess = AfterAccessNotification;
            BeforeAccess = BeforeAccessNotification;
            BeforeWrite = BeforeWriteNotification;

            // look up the entry in the DB
            using (var dbContext = new PlatformDBContext())
            {
                _cache = dbContext.ActiveDirectoryCache.FirstOrDefault(c => c.UniqueId == _user);
            }

            // place the entry in memory
            Deserialize(_cache == null ? null : _cache.CacheBits);
        }

        // clean up the DB
        public override void Clear()
        {
            base.Clear();

            using (var dbContext = new PlatformDBContext())
            {
                foreach (var cacheEntry in dbContext.ActiveDirectoryCache)
                    dbContext.ActiveDirectoryCache.Remove(cacheEntry);

                dbContext.SaveChanges();
            }
        }

        // Notification raised before ADAL accesses the cache.
        // This is your chance to update the in-memory copy from the DB, if the in-memory version is stale
        void BeforeAccessNotification(TokenCacheNotificationArgs args)
        {
            using (var dbContext = new PlatformDBContext())
            {
                if (_cache == null)
                {
                    // first time access
                    _cache = dbContext.ActiveDirectoryCache.FirstOrDefault(c => c.UniqueId == _user);
                }
                else
                {
                    // retrieve last write from the DB
                    var status = from e in dbContext.ActiveDirectoryCache
                                 where (e.UniqueId == _user)
                                 select new
                                 {
                                     LastWrite = e.LastWrite
                                 };
                    // if the in-memory copy is older than the persistent copy
                    if (status.First().LastWrite > _cache.LastWrite)
                    //// read from from storage, update in-memory copy
                    {
                        _cache = dbContext.ActiveDirectoryCache.FirstOrDefault(c => c.UniqueId == _user);
                    }
                }
            }

            Deserialize(_cache == null ? null : _cache.CacheBits);
        }

        // Notification raised after ADAL accessed the cache.
        // If the HasStateChanged flag is set, ADAL changed the content of the cache
        void AfterAccessNotification(TokenCacheNotificationArgs args)
        {
            using (var dbContext = new PlatformDBContext())
            {
                // if state changed
                if (HasStateChanged)
                {
                    _cache = new ActiveDirectoryCache
                    {
                        UniqueId = _user,
                        CacheBits = this.Serialize(),
                        LastWrite = DateTime.Now
                    };
                    //// update the DB and the lastwrite                
                    dbContext.Entry(_cache).State = _cache.Id == 0 ? EntityState.Added : EntityState.Modified;
                    dbContext.SaveChanges();
                    HasStateChanged = false;
                }
            }
        }

        void BeforeWriteNotification(TokenCacheNotificationArgs args)
        {
            // if you want to ensure that no concurrent write take place, use this notification to place a lock on the entry
        }
    }*/
}