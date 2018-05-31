using IdentityModel;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Auth.Models;
using PrimeApps.Auth.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace PrimeApps.Auth.Controllers
{
	public class UserController : Controller
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly SignInManager<ApplicationUser> _signInManager;

		public UserController(
			UserManager<ApplicationUser> userManager,
			SignInManager<ApplicationUser> signInManager)
		{
			_userManager = userManager;
			_signInManager = signInManager;
		}

		[HttpPost]
		public IActionResult Register([FromBody]RegisterViewModel registerViewModel)
		{
			if (!ModelState.IsValid)
				return BadRequest(ModelState);

			var userCheck = _userManager.FindByNameAsync(registerViewModel.Email).Result;

			if (userCheck == null)
			{
				userCheck = new ApplicationUser
				{
					UserName = registerViewModel.Email,
					Email = registerViewModel.Email,
					PhoneNumber = registerViewModel.PhoneNumber,
					NormalizedEmail = registerViewModel.Email,
					NormalizedUserName = !string.IsNullOrEmpty(registerViewModel.FirstName) ? registerViewModel.FirstName + " " + registerViewModel.LastName : ""
				};
				var result = _userManager.CreateAsync(userCheck, registerViewModel.Password).Result;
				if (!result.Succeeded)
				{
					return BadRequest("User not created error is : " + result.Errors.First().Description);
				}

				result = _userManager.AddClaimsAsync(userCheck, new Claim[]{
						new Claim(JwtClaimTypes.Name, !string.IsNullOrEmpty(registerViewModel.FirstName) ? registerViewModel.FirstName + " " + registerViewModel.LastName : ""),
						new Claim(JwtClaimTypes.GivenName, registerViewModel.FirstName),
						new Claim(JwtClaimTypes.FamilyName, registerViewModel.LastName),
						new Claim(JwtClaimTypes.Email, registerViewModel.Email),
						new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean)
					}).Result;

				if (!result.Succeeded)
				{
					return Ok("User created but claims not generated");
				}
				return Ok("User created");
			}
			return BadRequest("User already exist");

		}

		[HttpPost]
		public IActionResult ChangePassword([FromBody]ChangePasswordViewModel changePasswordViewModel)
		{
			return Ok();
		}
	}
}
