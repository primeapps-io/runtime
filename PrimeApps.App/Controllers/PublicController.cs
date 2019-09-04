using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [AllowAnonymous]
    [Route("api/Public")]
    public class PublicController : ApiBaseController
    {
        private IPlatformUserRepository _platformUserRepository;
        private ITenantRepository _tenantRepository;
        public PublicController(IPlatformUserRepository platformUserRepository, ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
            _platformUserRepository = platformUserRepository;
        }

		/// <summary>
		/// Detects the culture of the client.
		/// </summary>
		/// <returns>System.String.</returns>
		[Route("DetectCulture")]
        [ProducesResponseType(typeof(string), 200)]
        //[ResponseType(typeof(string))]
        [HttpPost]
        public IActionResult DetectCulture()
		{
		    var user = _platformUserRepository.Get("caglar@ofisim.com");
            return Ok(Thread.CurrentThread.CurrentCulture.Name);
        }


        /// <summary>
        /// Checks if the email address is unique, and not registered in the system.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns><c>true</c> if the email is unique otherwise, <c>false</c>.</returns>
        [Route("IsUniqueEmail")]
        [ProducesResponseType(typeof(bool), 200)]
        //[ResponseType(typeof(bool))]
        [HttpGet]
        public async Task<IActionResult> IsUniqueEmail([FromQuery(Name = "email")]string email, [FromQuery(Name = "appId")]int appId)
        {
            //check it in the entity layer and return the result.
            var result = await _platformUserRepository.IsEmailAvailable(email, appId);
            return Ok(result);
        }

        [Route("GetCustomInfo")]
        [HttpGet]
        public async Task<IActionResult> GetCustomInfo([FromQuery(Name = "customDomain")]string customDomain)
        {
            var tenant = await _tenantRepository.GetByCustomDomain(customDomain);

            if (tenant == null)
                return Ok();

			//TODO Changed
            var customInfo = new CustomInfoDTO();
            customInfo.Logo = tenant.Setting.Logo;
            customInfo.Title = tenant.Setting.CustomTitle;
            customInfo.Description = tenant.Setting.CustomDescription;
            customInfo.Favicon = tenant.Setting.CustomFavicon;
            customInfo.Color = tenant.Setting.CustomColor;
            customInfo.Image = tenant.Setting.CustomImage;
            customInfo.Language = tenant.Setting.Language;

            return Ok(customInfo);
        }
    }
}
