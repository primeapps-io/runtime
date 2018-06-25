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
using PrimeApps.App.Storage;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Controllers
{
    [Route("api/Instance")]
    public class InstanceController : ApiBaseController
    {
        private IUserRepository _userRepository;
        private Warehouse _warehouse;
        private ITenantRepository _tenantRepository;
        private IPlatformUserRepository _platformUserRepository;
        private IConfiguration _configuration;

        public InstanceController(IUserRepository userRepository, Warehouse warehouse, ITenantRepository tenantRepository, IPlatformUserRepository platformUserRepository, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _warehouse = warehouse;
            _tenantRepository = tenantRepository;
            _platformUserRepository = platformUserRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_userRepository);

			base.OnActionExecuting(context);
		}

		/// <summary>
		/// Updates instance.
		/// </summary>
		/// <param name="tenantDto">The instance.</param>
		[Route("Edit")]
        [ProducesResponseType(typeof(void), 200)]
        //[ResponseType(typeof(void))]
        [HttpPost]
        public async Task<IActionResult> Edit([FromBody]TenantDTO tenantDto)
        {
            //check if the tenant id is valid, within the current session's context.
            var tenantToUpdate = await _tenantRepository.GetAsync(AppUser.TenantId);

            if (tenantToUpdate.OwnerId != AppUser.Id)
            {
                //it is an unauthorized request, block it by sending forbidden status code.
                return Forbid();
                //return new ForbiddenResult(Request);
            }

            //if it is valid, then update the changed fields.
            tenantToUpdate.Title = tenantDto.Title;
            tenantToUpdate.Setting.Currency = tenantDto.Currency;
            tenantToUpdate.Setting.Logo = tenantDto.Logo;
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
            }

            return Ok();
        }

        /// <summary>
        /// Gets work groups that have a relation with user
        /// </summary>
        /// <returns>WorkgroupsResult.</returns>
        [Route("GetWorkgroup")]
        [ProducesResponseType(typeof(WorkgroupsResult), 200)]
        //[ResponseType(typeof(WorkgroupsResult))]
        [HttpPost]
        public async Task<IActionResult> GetWorkgroup()
        {
            var result = new WorkgroupsResult();
            //get personal instances
            result.Personal = await _platformUserRepository.MyWorkgroups(AppUser.TenantId);

            return Ok(result);
        }

        /// <summary>
        /// Lets the administrator of the workgroup dismiss any user or invited email address out of group.
        /// </summary>
        /// <param name="relation">relation object</param>
        [Route("Dismiss")]
        [ProducesResponseType(typeof(void), 200)]
        //[ResponseType(typeof(void))]
        [HttpPost]
        public async Task<IActionResult> Dismiss([FromBody]DismissDTO relation)
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
        

        [Route("SaveLogo")]
        [ProducesResponseType(typeof(void), 200)]
        //[ResponseType(typeof(void))]
        [HttpPost]
        public async Task<IActionResult> SaveLogo([FromBody]JObject logo)
        {
            var instanceToUpdate = await _tenantRepository.GetAsync(AppUser.TenantId);

            if (instanceToUpdate.OwnerId != AppUser.TenantId)
            {
                return Forbid();
                //return new ForbiddenResult(Request);
            }

            instanceToUpdate.Setting.Logo = (string)logo["url"];
            await _tenantRepository.UpdateAsync(instanceToUpdate);
            return Ok();
        }
    }
}
