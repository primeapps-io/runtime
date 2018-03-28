using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.App.Controllers
{
    [Route("api/platform"), Authorize, SnakeCase]
    public class PlatformController : BaseController
    {
        private IPlatformRepository _platformRepository;
        private IUserRepository _userRepository;
        private IProfileRepository _profileRepository;
        private IRoleRepository _roleRepository;
        private IRecordRepository _recordRepository;
        private ITenantRepository _tenantRepository;
        private IPlatformUserRepository _platformUserRepository;
        private Warehouse _warehouse;

        public PlatformController(IPlatformRepository platformRepository, IUserRepository userRepository, IProfileRepository profileRepository, IRoleRepository roleRepository, IRecordRepository recordRepository, ITenantRepository tenantRepository, IPlatformUserRepository platformUserRepository, Warehouse warehouse)
        {
            _platformRepository = platformRepository;
            _userRepository = userRepository;
            _profileRepository = profileRepository;
            _roleRepository = roleRepository;
            _warehouse = warehouse;
            _recordRepository = recordRepository;
            _tenantRepository = tenantRepository;
            _platformUserRepository = platformUserRepository;
        }

        [Route("app_get_by_id/{id:int}"), HttpGet]
        public async Task<IActionResult> GetApp(int id)
        {
            var userId = AppUser.Id;

            if (!AppUser.HasAdminProfile)
            {
                var currentUser = await _userRepository.GetById(userId);
                var adminUser = await _userRepository.GetByEmail(currentUser.CreatedByEmail);
                userId = adminUser.Id;
            }

            var appEntity = await _platformRepository.AppGetById(id, userId);

            if (appEntity == null)
                return NotFound();

            if (appEntity.Logo != null && !appEntity.Logo.StartsWith("http://"))
                appEntity.Logo = Storage.GetLogoUrl(appEntity.Logo);

            return Ok(appEntity);
        }

        [Route("app_get_all"), HttpGet]
        public async Task<IActionResult> GetAllApp()
        {
            var userId = AppUser.Id;

            if (!AppUser.HasAdminProfile)
            {
                var currentUser = await _userRepository.GetById(userId);
                var adminUser = await _userRepository.GetByEmail(currentUser.CreatedByEmail);
                userId = adminUser.Id;
            }

            var appEntities = await _platformRepository.AppGetAll(userId);

            foreach (var item in appEntities)
            {
                if (item.Logo != null && !item.Logo.StartsWith("http://"))
                    item.Logo = Storage.GetLogoUrl(item.Logo);
            }

            return Ok(appEntities);
        }

        [Route("app_create"), HttpPost]
        public async Task<IActionResult> CreateApp(AppBindingModel app)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var appEntity = await PlatformHelper.CreateEntity(app, _userRepository);
            var result = await _platformRepository.AppCreate(appEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            var userId = AppUser.Id;

            if (!AppUser.HasAdminProfile)
            {
                var currentUser = await _userRepository.GetById(userId);
                var adminUser = await _userRepository.GetByEmail(currentUser.CreatedByEmail);
                userId = adminUser.Id;
            }

            appEntity = await _platformRepository.AppGetById(appEntity.Id, (int)userId);

            if (!appEntity.TemplateId.HasValue)
                appEntity.TemplateId = 0;

            var appUser = AppUser;
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            await PlatformHelper.AppAfterCreate(appUser, appEntity, userManager, _userRepository, _profileRepository, _roleRepository, _recordRepository, _platformUserRepository, _tenantRepository, _warehouse);

            var uri = Request.RequestUri;
            return Created(uri.Scheme + "://" + uri.Authority + "/api/app/get_app/" + appEntity.Id, appEntity);
        }

        [Route("app_update/{id:int}"), HttpPut]
        public async Task<IActionResult> UpdateApp([FromRoute]int id, [FromBody]AppBindingModel app)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = AppUser.Id;

            if (!AppUser.HasAdminProfile)
            {
                var currentUser = await _userRepository.GetById(userId);
                var adminUser = await _userRepository.GetByEmail(currentUser.CreatedByEmail);
                userId = adminUser.Id;
            }

            var appEntity = await _platformRepository.AppGetById(id, userId);

            if (appEntity == null)
                return NotFound();

            await PlatformHelper.UpdateEntity(app, appEntity, _userRepository);
            await _platformRepository.AppUpdate(appEntity);

            return Ok(appEntity);
        }

        [Route("app_delete/{id:int}"), HttpDelete]
        public async Task<IActionResult> DeleteApp([FromRoute]int id)
        {
            var userId = AppUser.Id;

            if (!AppUser.HasAdminProfile)
            {
                var currentUser = await _userRepository.GetById(userId);
                var adminUser = await _userRepository.GetByEmail(currentUser.CreatedByEmail);
                userId = adminUser.Id;
            }

            var appEntity = await _platformRepository.AppGetById(id, userId);

            if (appEntity == null)
                return NotFound();

            await _platformRepository.AppDeleteSoft(appEntity);

            return Ok();
        }

        [Route("app_logo_upload"), HttpPost]
        public async Task<IActionResult> UploadLogo()
        {
            var requestStream = await Request.Content.ReadAsStreamAsync();
            var parser = new HttpMultipartParser(requestStream, "file");

            if (parser.Success)
            {
                if (parser.FileContents.Length <= 0)
                    return BadRequest();


                var chunk = 0;
                var chunks = 1;
                var uniqueName = string.Empty;

                if (parser.Parameters.Count > 1)
                {
                    chunk = int.Parse(parser.Parameters["chunk"]);
                    chunks = int.Parse(parser.Parameters["chunks"]);

                    if (parser.Parameters.ContainsKey("name"))
                        uniqueName = parser.Parameters["name"];
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid() + ext;
                }

                Storage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType);

                if (chunk == chunks - 1)
                {
                    var logo = $"{AppUser.TenantGuid}_{uniqueName}";
                    Storage.CommitFile(uniqueName, logo, parser.ContentType, "app-logo", chunks);
                    return Ok(logo);
                }

                return Ok(parser.ContentType);
            }

            return Ok("Fail");
        }

        [Route("office_app_create"), HttpGet]
        public async Task<IActionResult> CreateOfficeApp(int appId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            /*var appEntity = await PlatformHelper.CreateEntity(app, _userRepository);
            var result = await _platformRepository.AppCreate(appEntity);*/

            /*if (result < 1)
                throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            appEntity = await _platformRepository.AppGetById(appEntity.Id);

            if (!appEntity.TemplateId.HasValue)
                appEntity.TemplateId = 0;*/

            var appUser = AppUser;
            var userManager = Request.GetOwinContext().GetUserManager<ApplicationUserManager>();

            await PlatformHelper.AddApp(appUser, appId, userManager, _tenantRepository, _platformUserRepository, _userRepository, _profileRepository, _roleRepository, _recordRepository, _warehouse);

            var uri = Request.RequestUri;
            return Ok();
        }
    }
}
