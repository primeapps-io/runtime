using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PrimeApps.Auth.Models;
using PrimeApps.Auth.UI;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrimeApps.Auth.Controllers
{
	[Route("user")]
	public class UserController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private IPlatformRepository _platformRepository;

		public UserController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IPlatformRepository platformRepository)
		{
			_userManager = userManager;
			_signInManager = signInManager;
			_platformRepository = platformRepository;
		}

		[Route("register/{organization}/{app}"), HttpPost]
		public async Task<IActionResult> Register([FromBody]RegisterViewModel registerViewModel, string organization, string app)
		{
			if (string.IsNullOrEmpty(organization) || string.IsNullOrEmpty(app))
			{
				ModelState.AddModelError("", "Your url must be like register/{organization}/{app}");
				return BadRequest(ModelState);
			}

			if (!ModelState.IsValid)
				return BadRequest(ModelState);


			var appInfo = _platformRepository.GetAppInfo(organization, app);

			if (appInfo == null)
			{
				ModelState.AddModelError("", "App is not avaiable.");
				return BadRequest(ModelState);
			}

			var userExist = true;
			var userCheck = _userManager.FindByNameAsync(registerViewModel.Email).Result;

			if (userCheck == null)
			{
				userExist = false;
				var result = await AddUser(registerViewModel);
				if (!string.IsNullOrWhiteSpace(result))
					return BadRequest(result);
			}

			var user = await _userManager.FindByNameAsync(registerViewModel.Email);
			var token = "";

			var culture = !string.IsNullOrEmpty(registerViewModel.Culture) ? registerViewModel.Culture : appInfo.App.Setting.Culture;

			var url = Request.Scheme + "://" + appInfo.App.Setting.Domain + "/api/account/activate?email= " + registerViewModel.Email +
				"&appId=" + appInfo.App.Id + "&culture=" + culture + "&firstName=" + registerViewModel.FirstName + "&lastName=" + registerViewModel.LastName;

			if ((!userExist || !user.EmailConfirmed) && registerViewModel.SendActivation)
			{
				token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
				url += "&token=" + token;
			}

			using (var httpClient = new HttpClient())
			{
				httpClient.BaseAddress = new Uri(url);
				httpClient.DefaultRequestHeaders.Accept.Clear();
				httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

				var response = await httpClient.GetAsync(url);

				if (!response.IsSuccessStatusCode)
					return BadRequest(response);

			}

			return Ok(@"{ message: 'User created', emailConfirmToken: '" + token + "' }");
		}

		[Route("add_user"), HttpPost]
		public async Task<IActionResult> AddUser([FromQuery(Name = "email")] string email, [FromQuery(Name = "password")] string password, [FromQuery(Name = "firstName")] string firstName = "", [FromQuery(Name = "lastName")] string lastName = "")
		{
			if (string.IsNullOrEmpty(email))
			{
				ModelState.AddModelError("", "email is required");
				return BadRequest(ModelState);
			}

			var model = new RegisterViewModel
			{
				Email = email,
				FirstName = firstName,
				LastName = lastName,
				Password = password
			};

			var result = await AddUser(model);
			if (!string.IsNullOrWhiteSpace(result))
				return BadRequest(result);

			var user = await _userManager.FindByNameAsync(model.Email);
			var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

			return Ok("@{ token: "+ token +"}");
		}

		[HttpGet]
		public IActionResult ChangePassword([FromQuery(Name = "token")] string token)
		{
			if (string.IsNullOrEmpty(token))
			{
				ModelState.AddModelError("", "token is required");
				return BadRequest(ModelState);
			}

			return Ok();
		}

		[HttpPost]
		public IActionResult ChangePassword([FromBody]ChangePasswordViewModel changePasswordViewModel)
		{
			return Ok();
		}

		public async Task<string> AddUser(RegisterViewModel registerViewModel)
		{
			var user = new ApplicationUser
			{
				UserName = registerViewModel.Email,
				Email = registerViewModel.Email,
				NormalizedEmail = registerViewModel.Email,
				NormalizedUserName = !string.IsNullOrEmpty(registerViewModel.FirstName) ? registerViewModel.FirstName + " " + registerViewModel.LastName : ""
			};
			var result = await _userManager.CreateAsync(user, registerViewModel.Password);
			if (!result.Succeeded)
				return "User not created error is : " + result.Errors.First().Description;


			result = _userManager.AddClaimsAsync(user, new Claim[]{
				new Claim(JwtClaimTypes.Name, !string.IsNullOrEmpty(registerViewModel.FirstName) ? registerViewModel.FirstName + " " + registerViewModel.LastName : ""),
				new Claim(JwtClaimTypes.GivenName, registerViewModel.FirstName),
				new Claim(JwtClaimTypes.FamilyName, registerViewModel.LastName),
				new Claim(JwtClaimTypes.Email, registerViewModel.Email),
				new Claim(JwtClaimTypes.EmailVerified, "false", ClaimValueTypes.Boolean)
			}).Result;

			return "";
		}
	}
}
