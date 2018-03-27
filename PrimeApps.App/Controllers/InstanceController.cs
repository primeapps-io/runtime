using System;
using System.IO;
using System.Threading.Tasks;
using PrimeApps.App.Helpers;
using PrimeApps.App.Results;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Instance;

namespace PrimeApps.App.Controllers
{
    [Route("api/Instance")]
    public class InstanceController : BaseController
    {
        private IUserRepository _userRepository;
        private Warehouse _warehouse;
        private ITenantRepository _tenantRepository;
        private IPlatformUserRepository _platformUserRepository;

        public InstanceController(IUserRepository userRepository, Warehouse warehouse, ITenantRepository tenantRepository, IPlatformUserRepository platformUserRepository)
        {
            _userRepository = userRepository;
            _warehouse = warehouse;
            _tenantRepository = tenantRepository;
            _platformUserRepository = platformUserRepository;
        }

        /// <summary>
        /// Updates instance.
        /// </summary>
        /// <param name="tenantDto">The instance.</param>
        [Route("Edit")]
        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<IActionResult> Edit(TenantDTO tenantDto)
        {
            //check if the tenant id is valid, within the current session's context.
            var tenantToUpdate = await _tenantRepository.GetAsync(tenantDto.TenantId);
            bool isOperationAllowed = await Cache.Tenant.CheckProfilesAdministrativeRights(tenantDto.TenantId, AppUser.Id);

            if (tenantToUpdate.OwnerId != AppUser.Id && !isOperationAllowed)
            {
                //it is an unauthorized request, block it by sending forbidden status code.
                return new ForbiddenResult(Request);
            }

            //if it is valid, then update the changed fields.
            tenantToUpdate.Title = tenantDto.Title;
            tenantToUpdate.Currency = tenantDto.Currency;
            tenantToUpdate.Logo = tenantDto.Logo;
            await _tenantRepository.UpdateAsync(tenantToUpdate);

            if (!string.IsNullOrEmpty(tenantDto.Language))
            {
                using (var dbContext = _userRepository.DbContext)
                {
                    var culture = tenantDto.Language == "en" ? "en-US" : "tr-TR";

                    foreach (var usr in dbContext.Users)
                    {
                        usr.Culture = culture;
                    }

                    dbContext.SaveChanges();
                }

                var instanceCache = await Cache.Tenant.Get(tenantDto.TenantId);

                foreach (var userId in instanceCache.Users)
                {
                    await Cache.User.Remove(userId);
                }

                await Cache.Tenant.Delete(tenantDto.TenantId);
            }

            return Ok();
        }

        /// <summary>
        /// Gets work groups that have a relation with user
        /// </summary>
        /// <returns>WorkgroupsResult.</returns>
        [Route("GetWorkgroup")]
        [ResponseType(typeof(WorkgroupsResult))]
        [HttpPost]
        public async Task<IActionResult> GetWorkgroup()
        {
            var result = new WorkgroupsResult();
            var defaultInstance = await Cache.Tenant.Get(AppUser.TenantId);
            var adminUser = await Cache.User.Get(defaultInstance.OwnerId);

            //get personal instances
            result.Personal = await _platformUserRepository.MyWorkgroups(adminUser.Id);

            return Ok(result);
        }

        /// <summary>
        /// Lets the administrator of the workgroup dismiss any user or invited email address out of group.
        /// </summary>
        /// <param name="relation">relation object</param>
        [Route("Dismiss")]
        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<IActionResult> Dismiss(DismissDTO relation)
        {
            TenantUser user = await _userRepository.GetByEmail(relation.EMail);

            if (user != null)
            {
                user.IsActive = false;

                //Set warehouse database name
                _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

                await _userRepository.UpdateAsync(user);
            }

            return Ok();
        }

        /// <summary>
        /// Uploads a new avatar for the user.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <returns>System.String.</returns>
        [Route("UploadLogo")]
        [ResponseType(typeof(string))]
        [HttpPost]
        public async Task<IActionResult> UploadLogo()
        {
            // try to parse stream.
            Stream requestStream = await Request.Content.ReadAsStreamAsync();
            HttpMultipartParser parser = new HttpMultipartParser(requestStream, "file");

            if (parser.Success)
            {
                //if succesfully parsed, then continue to thread.
                if (parser.FileContents.Length <= 0)
                {
                    //if file is invalid, then stop thread and return bad request status code.
                    return BadRequest();
                }

                //initialize chunk parameters for the upload.
                int chunk = 0;
                int chunks = 1;

                var uniqueName = string.Empty;

                if (parser.Parameters.Count > 1)
                {
                    //this is a chunked upload process, calculate how many chunks we have.
                    chunk = int.Parse(parser.Parameters["chunk"]);
                    chunks = int.Parse(parser.Parameters["chunks"]);

                    //get the file name from parser
                    if (parser.Parameters.ContainsKey("name"))
                        uniqueName = parser.Parameters["name"];
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid() + ext;
                }

                //upload file to the temporary storage.
                Storage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType);

                if (chunk == chunks - 1)
                {
                    //if this is last chunk, then move the file to the permanent storage by commiting it.
                    //as a standart all avatar files renamed to UserID_UniqueFileName format.
                    var logo = string.Format("{0}_{1}", AppUser.TenantGuid, uniqueName);
                    Storage.CommitFile(uniqueName, logo, parser.ContentType, "company-logo", chunks);
                    return Ok(logo);
                }

                //return content type.
                return Ok(parser.ContentType);
            }
            //this is not a valid request so return fail.
            return Ok("Fail");
        }

        [Route("SaveLogo")]
        [ResponseType(typeof(void))]
        [HttpPost]
        public async Task<IActionResult> SaveLogo(JObject logo)
        {
            var instanceToUpdate = await _tenantRepository.GetAsync(AppUser.TenantId);
            var isOperationAllowed = await Cache.Tenant.CheckProfilesAdministrativeRights(AppUser.TenantId, AppUser.Id);

            if (instanceToUpdate.OwnerId != AppUser.TenantId && !isOperationAllowed)
            {
                return new ForbiddenResult(Request);
            }

            instanceToUpdate.Logo = (string)logo["url"];
            await _tenantRepository.UpdateAsync(instanceToUpdate);
            return Ok();
        }
    }
}
