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
using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using PrimeApps.Studio.Services;
using PrimeApps.Studio.Storage;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/register"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class RegisterController : Controller
    {
        private IConfiguration _configuration;
        private IOrganizationRepository _organizationRepository;
        private IConsoleUserRepository _consoleUserRepository;
        private IUnifiedStorage _storage;
        private IGiteaHelper _giteaHelper;

        public IBackgroundTaskQueue Queue { get; }

        public RegisterController(IConfiguration configuration,
            IOrganizationRepository organizationRepository,
            IConsoleUserRepository consoleUserRepository,
            IUnifiedStorage storage,
            IGiteaHelper giteaHelper,
            IBackgroundTaskQueue queue)
        {
            _organizationRepository = organizationRepository;
            _consoleUserRepository = consoleUserRepository;
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

            var consoleUser = new Model.Entities.Console.ConsoleUser
            {
                Id = id,
                UserOrganizations = new List<OrganizationUser>()
            };

            var result = await _consoleUserRepository.Create(consoleUser);
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
                    OwnerId = consoleUser.Id,
                    CreatedById = consoleUser.Id,
                    Default = true,
                    OrganizationUsers = new List<OrganizationUser>()
                };

                organization.OrganizationUsers.Add(new OrganizationUser
                {
                    UserId = consoleUser.Id,
                    Role = OrganizationRole.Administrator,
                    CreatedById = consoleUser.Id,
                    CreatedAt = DateTime.Now
                });

                await _organizationRepository.Create(organization);

                await _giteaHelper.CreateUser(user.Email, user.Password, user.FirstName, user.LastName, orgName);
            }

            return Ok(organization.Id);
        }
    }
}
