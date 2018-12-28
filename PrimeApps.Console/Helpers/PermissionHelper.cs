using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Console.Helpers
{
    public interface IPermissionHelper
    {
        Task<bool> CheckUserRole(int userId, int organizationId, OrganizationRole role);
    }

    public class PermissionHelper : IPermissionHelper
    {

        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IOrganizationUserRepository _organizationUserRepository;
        private IConfiguration _configuration;

        public PermissionHelper(IHttpContextAccessor context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IOrganizationUserRepository organizationUserRepository)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _organizationUserRepository = organizationUserRepository;
        }

        public async Task<bool> CheckUserRole(int userId, int organizationId, OrganizationRole role)
        {
            var userRole = await _organizationUserRepository.GetUserRole(userId, organizationId);

            return userRole == role;
        }
    }
}
