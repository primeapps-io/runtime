using StackExchange.Redis.Extensions.Core;
using System;
using System.Threading.Tasks;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;

namespace PrimeApps.App.Cache
{
    /// <summary>
    /// Manages  email address and user id relation for users.
    /// </summary>
    public static class ApplicationUser
    {
        static StackExchangeRedisCacheClient _cacheClient = Redis.Client();
        /// <summary>
        /// Adds a new application user record to the cache.
        /// </summary>
        /// <param name="apiKey"></param>
        /// <param name="userID"></param>
        public async static Task Add(string email, int userID)
        {
            await _cacheClient.AddAsync(string.Format("appUser_{0}", email), userID.ToString(), TimeSpan.FromDays(14));
        }

        /// <summary>
        /// Removes an existing application user entry from cache.
        /// </summary>
        /// <param name="apiKey"></param>
        public async static Task Remove(string email)
        {
            await _cacheClient.RemoveAsync(string.Format("appuser{0}", email));
        }

        /// <summary>
        /// Returns application user id by email
        /// </summary>
        /// <param name="apiKey"></param>
        /// <returns></returns>
        public async static Task<int> GetId(string email)
        {
            string value;
            int userID;
            value = await _cacheClient.GetAsync<string>(string.Format("appUser_{0}", email));

            if (value == null)
            {
                using (var pDbContext = new PlatformDBContext())
                {
                    using (PlatformUserRepository puRepository = new PlatformUserRepository(pDbContext))
                    {
                        userID = await puRepository.GetIdByEmail(email);
                    }
                }
                if (userID != 0)
                {
                    await Add(email, userID);
                }
            }
            else
            {
                int.TryParse(value, out userID);
            }

            return userID;
        }
    }
}