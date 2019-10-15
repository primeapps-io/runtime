using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Admin.Helpers
{
    public interface IOrganizationHelper
    {
        Task<OrganizationModel> GetById(int id, string token);
        Task<List<OrganizationModel>> Get(string token);
        bool ReloadOrganization();
    }

    public class OrganizationHelper : IOrganizationHelper
    {
        private readonly string _organizationKey = "primeapps_admin_organizations";
        private readonly IConfiguration _configuration;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ICacheHelper _cacheHelper;

        public OrganizationHelper(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration, ICacheHelper cacheHelper)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
            _cacheHelper = cacheHelper;
        }

        public async Task<List<OrganizationModel>> Get(string token)
        {
            var organizationString = await _cacheHelper.GetAsync<List<OrganizationModel>>(_organizationKey);

            if (organizationString != null)
                return organizationString;

            var studioClient = new StudioClient(_configuration, token);
            var organizations = await studioClient.OrganizationGetAllByUser();

            _cacheHelper.SetAsync(_organizationKey, organizations);

            return organizations;
        }

        public async Task<OrganizationModel> GetById(int id, string token)
        {
            var organizations = await _cacheHelper.GetAsync<List<OrganizationModel>>(_organizationKey);

            if (organizations != null)
                return organizations.FirstOrDefault(x => x.Id == id);

            var studioClient = new StudioClient(_configuration, token);
            organizations = await studioClient.OrganizationGetAllByUser();

            _cacheHelper.SetAsync(_organizationKey, organizations);

            return organizations.FirstOrDefault(x => x.Id == id);
        }

        public bool ReloadOrganization()
        {
            var organization = _cacheHelper.Get<List<OrganizationModel>>(_organizationKey);

            if (organization == null)
                return false;

            _cacheHelper.Remove(_organizationKey);
            return true;
        }
    }
}