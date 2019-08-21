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
    public interface IOrganizationHelper
    {
        Task<List<OrganizationModel>> Get(int userId);
        Task<bool> ReloadOrganization();
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

        public async Task<List<OrganizationModel>> Get(int userId)
        {
            var organizationString = _redisHelper.Get(_organizationRedisKey);

            if (!string.IsNullOrEmpty(organizationString))
                return JsonConvert.DeserializeObject<List<OrganizationModel>>(organizationString);

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                using (var appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                using (var organizationRespository = new OrganizationRepository(databaseContext, _configuration))
                {
                    var organizationUsers = await organizationRespository.GetByUserId(userId);

                    if (organizationUsers.Count < 1)
                        return null;

                    var organizations = new List<OrganizationModel>();

                    foreach (var organizationUser in organizationUsers)
                    {
                        var apps = await appDraftRepository.GetUserApps(userId, organizationUser.Organization.Id);

                        var organization = new OrganizationModel
                        {
                            Id = organizationUser.Organization.Id,
                            Label = organizationUser.Organization.Label,
                            Name = organizationUser.Organization.Name,
                            OwnerId = organizationUser.Organization.OwnerId,
                            Default = organizationUser.Organization.Default,
                            Icon = organizationUser.Organization.Icon,
                            Color = organizationUser.Organization.Color,
                            CreatedAt = organizationUser.Organization.CreatedAt,
                            CreatedById = organizationUser.Organization.CreatedById,
                            Role = organizationUser.Role,
                            Apps = apps
                        };

                        organizations.Add(organization);
                    }

                    _redisHelper.Set(_organizationRedisKey, organizations.ToJsonString());

                    return organizations;
                }
            }
        }
        
        public async Task<bool> ReloadOrganization()
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