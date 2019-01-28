using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Console.Helpers
{
    public interface IOrganizationHelper
    {
        Task<ICollection<OrganizationUserModel>> CreateCollaborators(List<OrganizationUser> users, int organizationId);
    }

    public class OrganizationHelper : IOrganizationHelper
    {
        public UserItem _appUser { get; set; }

        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;

        public OrganizationHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;

        }

        public async Task<ICollection<OrganizationUserModel>> CreateCollaborators(List<OrganizationUser> users, int organizationId)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<PlatformDBContext>();
                var cacheHelper = _scope.ServiceProvider.GetRequiredService<ICacheHelper>();

                var collaborators = new List<OrganizationUserModel>();

                using (var _platformUserRepository = new PlatformUserRepository(databaseContext, _configuration, cacheHelper))
                {
                    foreach (var user in users)
                    {
                        var platformUser = await _platformUserRepository.GetSettings(user.UserId);

                        if (platformUser != null)
                        {
                            collaborators.Add(new OrganizationUserModel
                            {
                                Id = user.UserId,
                                OrganizationId = organizationId,
                                Role = user.Role,
                                Email = platformUser.Email,
                                FirstName = platformUser.FirstName,
                                LastName = platformUser.LastName,
                                FullName = platformUser.FirstName + " " + platformUser.LastName,
                                CreatedAt = platformUser.CreatedAt,
                            });
                        }
                    }
                }

                return collaborators;
            }
        }
    }
}
