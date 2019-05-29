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
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Organization;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Storage;
using PrimeApps.Util.Storage;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/user"), Authorize(AuthenticationSchemes = "Bearer"), ActionFilters.CheckHttpsRequire, ResponseCache(CacheProfileName = "Nocache")]
    public class UserController : BaseController
    {
        private IConfiguration _configuration;
        private IPlatformUserRepository _platformUserRepository;
        private IAppDraftRepository _appDraftRepository;
        private IOrganizationRepository _organizationRepository;
        private ITeamRepository _teamRepository;
        private IStudioUserRepository _studioUserRepository;
        private IUnifiedStorage _storage;

        public UserController(IConfiguration configuration, IPlatformUserRepository platformUserRepository, IAppDraftRepository appDraftRepository, IOrganizationRepository organizationRepository, ITeamRepository teamRepository, IStudioUserRepository consoleUserRepository, IUnifiedStorage storage)
        {
            _platformUserRepository = platformUserRepository;
            _appDraftRepository = appDraftRepository;
            _organizationRepository = organizationRepository;
            _teamRepository = teamRepository;
            _studioUserRepository = consoleUserRepository;
            _configuration = configuration;
            _storage = storage;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContextUser();

            SetCurrentUser(_platformUserRepository);
            SetCurrentUser(_appDraftRepository);
            SetCurrentUser(_organizationRepository);
            SetCurrentUser(_studioUserRepository);
            SetCurrentUser(_teamRepository);

        }

        [Route("me"), HttpGet]
        public async Task<IActionResult> Me()
        {
            var user = await _platformUserRepository.GetWithSettings(AppUser.Email);


            var me = new Model.Common.User.StudioUser
            {
                Id = AppUser.Id,
                Email = user.Email,
                FirstName = user.FirstName,
                LastName = user.LastName,
                FullName = user.GetFullName(),
                Phone = user.Setting.Phone,
                Picture = user.ProfilePicture
            };

            return Ok(me);
        }

        [Route("apps"), HttpPost]
        public async Task<IActionResult> Apps([FromBody]JObject request)
        {
            var search = "";
            var page = 0;
            var status = PublishStatus.NotSet;

            if (request != null)
            {
                if (!request["search"].IsNullOrEmpty())
                    search = request["search"].ToString();

                if (!request["page"].IsNullOrEmpty())
                    page = (int)request["page"];

                if (!request["status"].IsNullOrEmpty())
                    status = (PublishStatus)int.Parse(request["status"].ToString());
            }

            var apps = await _appDraftRepository.GetAllByUserId(AppUser.Id, search, page, status);

            return Ok(apps);
        }

        [Route("organizations"), HttpGet]
        public async Task<IActionResult> Organizations()
        {
            var organizationUsers = await _organizationRepository.GetByUserId(AppUser.Id);

            if (organizationUsers.Count < 1)
                return Ok(null);

            List<OrganizationModel> organizations = new List<OrganizationModel>();

            foreach (var organizationUser in organizationUsers)
            {
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
                    Role = organizationUser.Role
                };

                organizations.Add(organization);
            }

            return Ok(organizations);
        }

        [Route("edit"), HttpPut]
        public async Task<IActionResult> PlatformUserUpdate([FromBody]PlatformUser user)
        {
            var platformUser = _platformUserRepository.GetByEmail(user.Email);

            platformUser = UserHelper.UpdatePlatformUser(platformUser, user);
            await _platformUserRepository.UpdateAsync(platformUser);

            return Ok(platformUser);
        }

        [Route("upload_profile_picture/{id:int}"), HttpPost]
        public async Task<IActionResult> UploadProfilePicture(int id)
        {
            HttpMultipartParser parser = new HttpMultipartParser(Request.Body, "file");
            StringValues bucketName = UnifiedStorage.GetPathPictures("profilepicture", id);

            if (parser.Success)
            {
                //if succesfully parsed, then continue to thread.
                if (parser.FileContents.Length <= 0)
                {
                    //if file is invalid, then stop thread and return bad request status code.
                    return BadRequest();
                }

                var uniqueName = string.Empty;
                //get the file name from parser
                if (parser.Parameters.ContainsKey("name"))
                {
                    uniqueName = parser.Parameters["name"];
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid() + ext;
                }

                var fileName = string.Format("{0}_{1}", AppUser.Id, uniqueName);

                using (Stream stream = new MemoryStream(parser.FileContents))
                {
                    await _storage.Upload(parser.Filename, bucketName, fileName, stream);
                }

                var logo = _storage.GetLink(bucketName, fileName);

                //return content type.
                return Ok(logo);
            }

            //this is not a valid request so return fail.
            return Ok("Fail");
        }

        [Route("count"), HttpGet]
        public IActionResult Count()
        {
            var value = 100;

            return Ok(value);
        }
    }
}
