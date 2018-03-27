using System;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Description;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [AllowAnonymous]
    [RoutePrefix("api/Public")]
    public class PublicController : BaseController
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
        [ResponseType(typeof(string))]
        [HttpPost]
        public IHttpActionResult DetectCulture()
        {
            return Ok(Thread.CurrentThread.CurrentCulture.Name);
        }


        /// <summary>
        /// Checks if the email address is unique, and not registered in the system.
        /// </summary>
        /// <param name="email">The email.</param>
        /// <returns><c>true</c> if the email is unique otherwise, <c>false</c>.</returns>
        [Route("IsUniqueEmail")]
        [ResponseType(typeof(bool))]
        [HttpGet]
        public IHttpActionResult IsUniqueEmail(string email)
        {
            //check it in the entity layer and return the result.
            var result = _platformUserRepository.IsEmailAvailable(email);
            return Ok(result);
        }

        [Route("GetCustomInfo")]
        [HttpGet]
        public async Task<IHttpActionResult> GetCustomInfo(string customDomain)
        {
            var cacheClient = Redis.Client();
            var customInfo = await cacheClient.GetAsync<CustomInfoDTO>($"custom_info_{customDomain.Replace(".", "")}");

            if (customInfo != null)
                return Ok(customInfo);

            var tenant = await _tenantRepository.GetByCustomDomain(customDomain);

            if (tenant == null)
                return Ok();

            customInfo = new CustomInfoDTO();
            customInfo.Logo = TenantRepository.GetLogoUrl(tenant.Logo);
            customInfo.Title = tenant.CustomTitle;
            customInfo.Description = tenant.CustomDescription;
            customInfo.Favicon = tenant.CustomFavicon;
            customInfo.Color = tenant.CustomColor;
            customInfo.Image = tenant.CustomImage;
            customInfo.Language = tenant.Language;

            await cacheClient.AddAsync($"custom_info_{customDomain.Replace(".", "")}", customInfo, TimeSpan.FromDays(90));

            return Ok(customInfo);
        }
    }
}
