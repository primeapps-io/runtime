using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Context;
using System.Data.Entity;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Entities.Platform.Identity;
using PrimeApps.Model.Repositories;

namespace PrimeApps.App.Cache
{
    /// <summary>
    /// User Session Provider, we keep user sessions by the help of this class.
    /// </summary>
    public static class User
    {
        static StackExchangeRedisCacheClient _cacheClient = Redis.Client();
        /// <summary>
        /// This method gets the session from session cache if it is not available by locally, it brings it from database.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public async static Task<UserItem> Get(int userId, bool createIfNotExists = true)
        {
            UserItem value = await _cacheClient.GetAsync<UserItem>(string.Format("user_{0}", userId));

            if (value == null && createIfNotExists)
            {
                // A user with an API key is requesting an access to our resources. But it's not in my memory cache.
                // Let's see if the information exists in DB.

                // The key exists in DB. Let's cache it in memory, so that the subsequent requests can be faster.
                PlatformUser user;

                using (var pDbContext = new PlatformDBContext())
                {
                    using (PlatformUserRepository puRepository = new PlatformUserRepository(pDbContext))
                    {
                        user = await puRepository.GetWithTenant(userId);
                    }
                }

                UserItem newCacheEntry = new UserItem();
                newCacheEntry.UserName = user.FirstName + " " + user.LastName;
                newCacheEntry.Email = user.Email;
                newCacheEntry.Currency = user.Currency;
                newCacheEntry.Id = user.Id;
                newCacheEntry.TenantId = user.TenantId.Value;
                newCacheEntry.AppId = user.AppId;
                newCacheEntry.SecurityStamp = user.SecurityStamp;
                newCacheEntry.TenantGuid = user.Tenant.GuidId;

                //create tenant sessions for the tenant in the user's session.
                //get session for the instance
                var tenant = await Tenant.Get(user.TenantId.Value);

                newCacheEntry.TenantLanguage = tenant.Language;
                newCacheEntry.Culture = tenant.Language == "en" ? "en-US" : "tr-TR";

                if (!tenant.Users.Contains(userId))
                {
                    //if instance's user list does not contain user's id, modify the array and add current user in it.
                    var users = tenant.Users;
                    Array.Resize<int>(ref users, tenant.Users.Length + 1);
                    tenant.Users = users;
                    tenant.Users[tenant.Users.Length - 1] = userId;

                    //update instance's session.
                    await Tenant.AddOrUpdate(user.TenantId.Value, tenant);
                }

                using (TenantDBContext dbContext = new TenantDBContext(user.TenantId.Value))
                {
                    var tenantUser = await dbContext.Users.Where(x => x.Id == user.Id).Include(x => x.Profile).Include(x => x.Role).SingleOrDefaultAsync();
                    newCacheEntry.HasAdminProfile = tenantUser.Profile.HasAdminRights;
                    newCacheEntry.ProfileId = tenantUser.ProfileId.Value;
                    newCacheEntry.Role = tenantUser.RoleId.Value;
                }

                //set warehouse database name
                if (tenant.HasAnalytics.HasValue && tenant.HasAnalytics.Value)
                {
                    Model.Entities.Platform.PlatformWarehouse warehouseEntity;
                    using (PlatformDBContext dbContext = new PlatformDBContext())
                    {
                        using (PlatformWarehouseRepository warehousePlatformRepository = new PlatformWarehouseRepository(dbContext))
                        {

                            warehouseEntity = await warehousePlatformRepository.GetByTenantId(user.TenantId.Value);
                        }
                    }
                    if (warehouseEntity != null)
                    {
                        newCacheEntry.WarehouseDatabaseName = warehouseEntity.DatabaseName;
                    }
                    else
                    {
                        newCacheEntry.WarehouseDatabaseName = "0"; //this is important. checking this warehouse database name set or not.
                    }
                }
                else
                {
                    newCacheEntry.WarehouseDatabaseName = "0";//this is important. checking this warehouse database name set or not.
                }

                await AddOrUpdate(userId, newCacheEntry);
                value = newCacheEntry;
            }
            //return value for the request.
            return value;
        }

        /// <summary>
        /// Adds or updates user session to global user session cache
        /// </summary>
        /// <param name="apiKey">The API key.</param>
        /// <param name="value">The value.</param>
        /// <param name="isGlobal">if set to <c>true</c> [is global].</param>
        public async static Task AddOrUpdate(int userId, UserItem value)
        {

            //add it to the cache
            await _cacheClient.AddAsync<UserItem>(string.Format("user_{0}", userId), value, TimeSpan.FromDays(90));

            //create tenant sessions for every tenant in the user's session.

            TenantItem currentTenant = await Tenant.Get(value.TenantId);

            if (!currentTenant.Users.Contains(userId))
            {
                //if instance's user list does not contain user's id, modify the array and add current user in it.
                var users = currentTenant.Users;
                Array.Resize<int>(ref users, currentTenant.Users.Length + 1);
                currentTenant.Users = users;
                currentTenant.Users[currentTenant.Users.Length - 1] = userId;

                //update instance's session.
                await Cache.Tenant.AddOrUpdate(value.TenantId, currentTenant);
            }
        }

        /// <summary>
        /// Updates the specified users sessions.
        /// </summary>
        /// <param name="userId">The user identifier.</param>
        /// <param name="value">User session object.</param>
        /// <param name="isGlobal">if set to <c>true</c> [is global].</param>
        public async static Task Update(int userId, UserItem value)
        {
            //replace the object.
            await AddOrUpdate(userId, value);
        }

        /// <summary>
        /// Removes user from global session cache
        /// </summary>
        /// <param name="instanceID"></param>
        public async static Task Remove(int userId)
        {
            await _cacheClient.RemoveAsync(string.Format("user_{0}", userId));
        }

        /// <summary>
        /// Removes given tenant from every users session.
        /// </summary>
        /// <param name="instanceId"></param>
        public async static Task DisposeInstance(int instanceId)
        {
            ////get sessions for the given instance
            TenantItem inst = await Tenant.Get(instanceId);

            foreach (int userId in inst.Users)
            {
                UserItem user = await Get(userId, false);
                if (user == null) return;

                //remove the tenant id from
                user.TenantId = 0;
                await AddOrUpdate(userId, user);
            }
        }

        /// <summary>
        /// Adds tenant to users session also adds users entry to joined instances session.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tenantId"></param>
        public async static Task JoinToInstance(int userId, int tenantId)
        {
            //get all instances that belong to the user
            UserItem user = await Get(userId);


            if (!user.TenantId.Equals(tenantId))
            {
                //update user cache
                await AddOrUpdate(userId, user);
            }

            //update tenant cache
            await Tenant.AddUser(userId, tenantId);
        }

        /// <summary>
        /// Removes tenant from users session also removes users entry from session of the tenant which the user has left.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="tenantId"></param>
        public async static Task LeaveInstance(int userId, int tenantId)
        {
            //get sessions for the user
            UserItem session = await Get(userId);

            if (session.TenantId.Equals(tenantId))
                await Remove(userId);

            //remove the user from tenant cache too
            await Tenant.RemoveUser(userId, tenantId);
        }
    }

}