using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Common.User;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using PrimeApps.Studio.Services;
using PrimeApps.Studio.Storage;
using StudioUser = PrimeApps.Model.Entities.Studio.StudioUser;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/register"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class RegisterController : Controller
    {
        private IConfiguration _configuration;
        private IOrganizationRepository _organizationRepository;
        private IStudioUserRepository _studioUserRepository;
        private IUnifiedStorage _storage;
        private IGiteaHelper _giteaHelper;

        public IBackgroundTaskQueue Queue { get; }

        public RegisterController(IConfiguration configuration,
            IOrganizationRepository organizationRepository,
            IStudioUserRepository studioUserRepository,
            IUnifiedStorage storage,
            IGiteaHelper giteaHelper,
            IBackgroundTaskQueue queue)
        {
            _organizationRepository = organizationRepository;
            _studioUserRepository = studioUserRepository;
            _configuration = configuration;
            _storage = storage;
            _giteaHelper = giteaHelper;
            Queue = queue;
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody] StudioUserBindingModel user)
        {
            if (string.IsNullOrEmpty(user.Id))
                return BadRequest("User id is required");

            var decryptId = CryptoHelper.Decrypt(user.Id);

            var validId = int.TryParse(decryptId, out int id);

            if (!validId)
                return BadRequest("Id is not valid");

            var studioUser = new StudioUser
            {
                Id = id,
                UserOrganizations = new List<OrganizationUser>()
            };

            var result = await _studioUserRepository.Create(studioUser);
            Organization organization = null;

            if (result >= 1)
            {
                var query = user.Email.Replace("@", "").Split(".");
                Array.Resize(ref query, query.Length - 1);
                var orgName = string.Join("", query);

                organization = new Organization
                {
                    Name = orgName,
                    Label = user.FirstName + " " + user.LastName,
                    OwnerId = studioUser.Id,
                    CreatedById = studioUser.Id,
                    Default = true,
                    OrganizationUsers = new List<OrganizationUser>()
                };

                organization.OrganizationUsers.Add(new OrganizationUser
                {
                    UserId = studioUser.Id,
                    Role = OrganizationRole.Administrator,
                    CreatedById = studioUser.Id,
                    CreatedAt = DateTime.Now
                });

                await _organizationRepository.Create(organization);

                await _giteaHelper.CreateUser(user.Email, user.Password, user.FirstName, user.LastName, orgName);
            }

            return Ok(organization.Id);
        }
    }
}
