using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [AllowAnonymous]
    [Route("api/Public")]
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
        [ProducesResponseType(typeof(string), 200)]
        //[ResponseType(typeof(string))]
        [HttpPost]
        public IActionResult DetectCulture()
        {
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
        public IActionResult IsUniqueEmail(string email)
        {
            //check it in the entity layer and return the result.
            var result = _platformUserRepository.IsEmailAvailable(email);
            return Ok(result);
        }

        [Route("GetCustomInfo")]
        [HttpGet]
        public async Task<IActionResult> GetCustomInfo(string customDomain)
        {
            var cacheClient = Redis.Client();
            var customInfo = await cacheClient.GetAsync<CustomInfoDTO>($"custom_info_{customDomain.Replace(".", "")}");

            if (customInfo != null)
                return Ok(customInfo);

            var tenant = await _tenantRepository.GetByCustomDomain(customDomain);

            if (tenant == null)
                return Ok();

            customInfo = new CustomInfoDTO();
            customInfo.Logo = TenantRepository.GetLogoUrl(tenant.Info.Logo);
            customInfo.Title = tenant.Info.CustomTitle;
            customInfo.Description = tenant.Info.CustomDescription;
            customInfo.Favicon = tenant.Info.CustomFavicon;
            customInfo.Color = tenant.Info.CustomColor;
            customInfo.Image = tenant.Info.CustomImage;
            customInfo.Language = tenant.Info.Language;

            await cacheClient.AddAsync($"custom_info_{customDomain.Replace(".", "")}", customInfo, TimeSpan.FromDays(90));

            return Ok(customInfo);
        }
    }
}
