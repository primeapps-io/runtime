using System;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/account")]
    public class AccountController : Controller
    {
        private IApplicationRepository _applicationRepository;

        public AccountController(IApplicationRepository applicationRepository)
        {
            _applicationRepository = applicationRepository;
        }

        [Route("logout")]
        public async Task<IActionResult> Logout()
        {
            var appInfo = await _applicationRepository.Get(Request.Host.Value);

            Response.Cookies.Delete("tenant_id");
            await HttpContext.SignOutAsync();

            return StatusCode(200, new {redirectUrl = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/Account/Logout?returnUrl=" + Request.Scheme + "://" + appInfo.Setting.AppDomain});
        }

		[Route("change_password")]
		public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordBindingModel changePasswordBindingModel)
		{
			if (HttpContext.User.FindFirst("email") == null || string.IsNullOrEmpty(HttpContext.User.FindFirst("email").Value))
				return Unauthorized();

			changePasswordBindingModel.Email = HttpContext.User.FindFirst("email").Value;

			var appInfo = await _applicationRepository.Get(Request.Host.Value);
			using (var httpClient = new HttpClient())
			{
				var url = Request.Scheme + "://" + appInfo.Setting.AuthDomain + "/user/change_password?client=" + appInfo.Name;
				httpClient.BaseAddress = new Uri(url);
				httpClient.DefaultRequestHeaders.Accept.Clear();
				httpClient.DefaultRequestHeaders.Add("Accept", "application/json");

				var json = JsonConvert.SerializeObject(changePasswordBindingModel);
				var response = await httpClient.PostAsync(url, new StringContent(json, Encoding.UTF8, "application/json"));

				if (!response.IsSuccessStatusCode)
					return BadRequest(response);
			}

			return Ok();
		}
	}
}