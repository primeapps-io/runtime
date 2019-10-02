using System;
using System.Collections.Generic; 
using System.Threading.Tasks; 
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PrimeApps.Model.Common.Organization; 

namespace PrimeApps.Admin.Helpers
{
    public interface IOrganizationHelper
    {
        Task<List<OrganizationModel>> Get(int userId, string token);
        bool ReloadOrganization();
    }

    public class OrganizationHelper : IOrganizationHelper
    {
        private readonly string _organizationRedisKey = "primeapps_admin_organizations";
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IRedisHelper _redisHelper;

        public OrganizationHelper(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, IRedisHelper redisHelper)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _redisHelper = redisHelper;
        }

        public async Task<List<OrganizationModel>> Get(int userId, string token)
        {
            var organizationString = _redisHelper.Get(_organizationRedisKey);

            if (!string.IsNullOrEmpty(organizationString))
                return JsonConvert.DeserializeObject<List<OrganizationModel>>(organizationString);

            var studioClient = new StudioClient(_configuration, token);
            var organizations = await studioClient.OrganizationGetAllByUser();

            return organizations;
        }

        public bool ReloadOrganization()
        {
            var organizationString = _redisHelper.Get(_organizationRedisKey);

            if (!string.IsNullOrEmpty(organizationString))
            {
                _redisHelper.Remove(_organizationRedisKey);
                return true;
            }
            else
                return false;
        }
    }
}