using StackExchange.Redis.Extensions.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Entities.Platform;

namespace PrimeApps.App.Cache
{
    /// <summary>
    /// Instance Session Provider, we keep instance sessions by the help of this class.
    /// It is a static class, so it's shared role-wide. Also we keep it in sync with other roles.
    /// </summary>
    public static class Tenant
    {
        static StackExchangeRedisCacheClient _cacheClient = Redis.Client();

        // Gets instance session.
        // This method returns instance information in the cache. If the information does not exist in memory, then
        // connects to other web role instances to see if they know about this instance.
        public async static Task<TenantItem> Get(int tenantId)
        {
            //try to get it from cache
            TenantItem value = await _cacheClient.GetAsync<TenantItem>(string.Format("tenant{0}", tenantId));


            if (value == null)
            {
                //if there is not entry for this tenant in the cache,
                //get it from database and push it to the session.
                value = new TenantItem();

                using (var pDbContext = new PlatformDBContext())
                {
                    using (TenantRepository tRepository = new TenantRepository(pDbContext))
                    {
                        Model.Entities.Platform.Tenant tenant = await tRepository.GetAsync(tenantId);
                        value.Language = tenant.Info.Language;
                        value.OwnerId = tenant.OwnerId;
                        value.HasAnalytics = tenant.Settings.AnalyticsLicenseCount > 0;
                        value.Users = tenant.Users.Select(x => x.UserId).ToArray();
                    }
                }


                using (TenantDBContext tDbContext = new TenantDBContext(tenantId))
                {
                    ProfileRepository profileRepository = new ProfileRepository(tDbContext);
                    RoleRepository roleRepository = new RoleRepository(tDbContext);

                    value.Profiles = await profileRepository.GetUserProfilesForCache();
                    value.Roles = await roleRepository.GetUserRolesForCache();
                }

                var now = DateTime.UtcNow;
                await AddOrUpdate(tenantId, value);
            }



            //return it to the user.
            return value;
        }

        /// <summary>
        /// Add or Update Session
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public async static Task AddOrUpdate(int key, TenantItem value)
        {
            await _cacheClient.AddAsync(string.Format("workgroup_{0}", key), value, TimeSpan.FromDays(90));
        }

        /// <summary>
        /// Remove Session with Session Users
        /// </summary>
        /// <param name="key"></param>
        public async static Task Remove(int tenantId)
        {
            //dispose instance record in the user sessions.
            await User.DisposeInstance(tenantId);
            await _cacheClient.RemoveAsync(string.Format("workgroup_{0}", tenantId));
        }

        /// <summary>
        /// Remove Session with Session Users
        /// </summary>
        /// <param name="key"></param>
        public async static Task Delete(int tenantId)
        {
            await _cacheClient.RemoveAsync(string.Format("workgroup_{0}", tenantId));
        }

        /// <summary>
        /// Adds user to the instance session.
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="tenantId"></param>
        public async static Task AddUser(int userID, int tenantId)
        {
            //Add new user to instance
            TenantItem cacheItem = await Get(tenantId);

            if (cacheItem.Users.Contains(userID)) return;

            List<int> users = cacheItem.Users.ToList();
            users.Add(userID);
            cacheItem.Users = users.ToArray();

            //replace with the new one.
            await AddOrUpdate(tenantId, cacheItem);
        }

        /// <summary>
        /// Removes user from instance session
        /// </summary>
        /// <param name="userID"></param>
        /// <param name="tenantId"></param>
        public async static Task RemoveUser(int userID, int tenantId)
        {
            //Remove user from instance
            var cacheItem = await Get(tenantId);

            if (!cacheItem.Users.Contains(userID)) return;

            var users = cacheItem.Users.ToList();
            users.Remove(userID);
            cacheItem.Users = users.ToArray();

            //replace it with the new one.
            await AddOrUpdate(tenantId, cacheItem);
        }

        /// <summary>
        /// Checks if a user has permission for a specific operation to an entity.
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="entityTypeID"></param>
        /// <param name="tenantId"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async static Task<bool> CheckPermission(PermissionEnum operation, int? moduleId, EntityType type, int tenantId, int userID)
        {
            TenantItem tenant = await Get(tenantId);
            bool isAllowed = false;
            if (tenant == null) return false;

            ProfileLightDTO profile = tenant.Profiles.Where(x => x.UserIDs.Contains(userID)).SingleOrDefault();
            if (profile == null) return false;

            ProfilePermissionLightDTO permission = profile.Permissions.Where(x => x.ModuleId == moduleId && x.Type == (int)type).SingleOrDefault();
            if (permission == null) return false;

            switch (operation)
            {
                case PermissionEnum.Write:
                    isAllowed = permission.Write;
                    break;
                case PermissionEnum.Read:
                    isAllowed = permission.Read;
                    break;
                case PermissionEnum.Remove:
                    isAllowed = permission.Remove;
                    break;
                case PermissionEnum.Modify:
                    isAllowed = permission.Modify;
                    break;
                default:
                    break;
            }

            return isAllowed;
        }
        /// <summary>
        /// Checks if the user profile has administrative rights for the given instance.
        /// </summary>
        /// <param name="tenantId"></param>
        /// <param name="userID"></param>
        /// <returns></returns>
        public async static Task<bool> CheckProfilesAdministrativeRights(int tenantId, int userID)
        {
            TenantItem tenant = await Get(tenantId);
            if (tenant == null) return false;

            ProfileLightDTO profile = tenant.Profiles.Where(x => x.UserIDs.Contains(userID)).SingleOrDefault();
            if (profile == null) return false;

            return profile.HasAdminRights;
        }

        /// <summary>
        /// Updates permission profiles for an instance.
        /// </summary>
        /// <param name="tenantId"></param>
        public async static Task UpdateProfiles(int tenantId)
        {
            TenantItem updatedTenantItem = await Get(tenantId);
            using (TenantDBContext dbContext = new TenantDBContext(tenantId))
            {
                ProfileRepository profileRepository = new ProfileRepository(dbContext);

                updatedTenantItem.Profiles = await profileRepository.GetUserProfilesForCache();
            }
            await AddOrUpdate(tenantId, updatedTenantItem);
        }

        /// <summary>
        /// Updates permission profiles for an instance.
        /// </summary>
        /// <param name="tenantId"></param>
        public async static Task UpdateRoles(int tenantId)
        {
            TenantItem updatedTenantItem = await Get(tenantId);

            using (TenantDBContext dbContext = new TenantDBContext(tenantId))
            {
                RoleRepository roleRepository = new RoleRepository(dbContext);

                updatedTenantItem.Roles = await roleRepository.GetUserRolesForCache();
            }
            await AddOrUpdate(tenantId, updatedTenantItem);
        }
    }
}