using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;

namespace PrimeApps.Admin.Helpers
{
    public interface IRedisHelper
    {
        string Get(string key);
        void Set(string key, string value);
        void Remove(string key);
    }

    public class RedisHelper : IRedisHelper
    {
        private readonly IDistributedCache _distributedCache;

        public RedisHelper(IDistributedCache distributedCache)
        {
            _distributedCache = distributedCache;
        }

        public string Get(string key)
        {
            var organizationString = _distributedCache.GetString(key);
            return !string.IsNullOrEmpty(organizationString) ? organizationString : null;
        }

        public void Set(string key, string value)
        {
            _distributedCache.SetString(key, value);
        }

        public void Remove(string key)
        {
            _distributedCache.Remove(key);
        }
    }
}