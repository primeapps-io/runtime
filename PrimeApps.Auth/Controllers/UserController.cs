using IdentityModel;
using IdentityServer4.Events;
using IdentityServer4.Extensions;
using IdentityServer4.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using PrimeApps.Auth.Models;
using PrimeApps.Auth.Models.UserViewModels;
using PrimeApps.Auth.UI;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace PrimeApps.Auth.Controllers
{
	[Route("user")]
	[SecurityHeaders]
	public class UserController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;
		private IPlatformRepository _platformRepository;
		private IApplicationRepository _applicationRepository;
		private readonly IEventService _events;
		public UserController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager,
			IEventService events,
			IPlatformRepository platformRepository,
			IApplicationRepository applicationRepository)
		{
			_applicationRepository = applicationRepository;
			_userManager = userManager;
			_signInManager = signInManager;
			_platformRepository = platformRepository;
			_events = events;
		}

		[Route("confirm_email"), HttpGet]
		public async Task<IActionResult> ConfirmEmail(string email, string token)
		{
			if (email == null || token == null)
			{
				ModelState.AddModelError("", "email and token are required.");
				return BadRequest(ModelState);
			}
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
			{
				throw new ApplicationException($"Unable to load user with email: '{email}'.");
			}
			var result = await _userManager.ConfirmEmailAsync(user, token);

			return result.Succeeded ? Ok() : (IActionResult)BadRequest(result.Errors.First().Description);
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


			var appInfo = await _applicationRepository.Get(organization, app);

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

			var url = Request.Scheme + "://" + appInfo.App.Setting.AppDomain.Replace("/ik", "") + "/api/account/create";

			var activateModel = new ActivateBindingModels
			{
				email = registerViewModel.Email,
				app_id = appInfo.App.Id,
				culture = culture,
				first_name = registerViewModel.FirstName,
				last_name = registerViewModel.LastName,
				email_confirmed = user.EmailConfirmed
			};

			if ((!userExist || !user.EmailConfirmed) && registerViewModel.SendActivation)
			{
				token = await GetConfirmToken(user);
				activateModel.token = token;
			}

			using (var httpClient = new HttpClient())
			{
				httpClient.BaseAddress = new Uri(url);
				httpClient.DefaultRequestHeaders.Accept.Clear();
				httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
				var response = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(activateModel), Encoding.UTF8, "application/json"));

				if (!response.IsSuccessStatusCode)
				{
					if (response.StatusCode == HttpStatusCode.Conflict)
						return Conflict(response);
					else
						return BadRequest(response);
				}
			}

			if (User?.Identity.IsAuthenticated == true)
			{
				// delete local authentication cookie
				await _signInManager.SignOutAsync();
				await _events.RaiseAsync(new UserLogoutSuccessEvent(User.GetSubjectId(), User.GetDisplayName()));
			}

			var signInResult = await _signInManager.PasswordSignInAsync(registerViewModel.Email, registerViewModel.Password, true, lockoutOnFailure: false);

			if (signInResult.Succeeded)
				await _events.RaiseAsync(new UserLoginSuccessEvent(user.UserName, user.Id, user.UserName));

			return Created(appInfo.App.Setting.AppDomain, new { token, domain = appInfo.App.Setting.AppDomain });
		}

		[Route("confirm_token"), HttpGet]
		public async Task<IActionResult> ConfirmTokenAsync(string email)
		{
			var user = await _userManager.FindByEmailAsync(email);
			if (user == null)
				return StatusCode(404, "{message:' user not found'}");
			else if (user.EmailConfirmed)
				return StatusCode(404, "{message: 'already confirmed'}");

			return StatusCode(200, "{token:'" + Json(await GetConfirmToken(user) + "'}"));
		}

		[Route("register"), HttpPost]
		public async Task<IActionResult> Register([FromBody]RegisterViewModel registerViewModel)
		{
			if (!ModelState.IsValid)
			{
				ModelState.AddModelError("", "ModelState is not valid.");
				return BadRequest(ModelState);
			}

			var user = await _userManager.FindByNameAsync(registerViewModel.Email);

			//If user already registered check email is confirmed. If not return confirm token with status code.
			if (user != null)
				return !user.EmailConfirmed ? StatusCode(201, new { token = await GetConfirmToken(user) }) : (IActionResult)StatusCode(201);


			var result = await AddUser(registerViewModel);
			if (!string.IsNullOrWhiteSpace(result))
				return BadRequest(result);

			user = await _userManager.FindByNameAsync(registerViewModel.Email);
			var token = await GetConfirmToken(user);

			return StatusCode(201, new { token });
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

		[Route("change_password"), HttpPost]
		public async Task<IActionResult> ChangePassword([FromBody]ChangePasswordViewModel changePasswordViewModel)
		{
			if (!ModelState.IsValid)
				return Unauthorized();

			var user = await _userManager.FindByEmailAsync(changePasswordViewModel.Email);
			var result = await _userManager.ChangePasswordAsync(user, changePasswordViewModel.OldPassword, changePasswordViewModel.NewPassword);

			return result.Succeeded ? Ok() : StatusCode(400);
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

		//Helpers
		public async Task<string> GetConfirmToken(ApplicationUser user)
		{
			return await _userManager.GenerateEmailConfirmationTokenAsync(user);
		}

	}
}
