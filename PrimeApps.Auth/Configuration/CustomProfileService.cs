using System.Security.Claims;
using System.Threading.Tasks;
using IdentityServer4.Extensions;
using IdentityServer4.Models;
using IdentityServer4.Services;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using PrimeApps.Auth.Models;
using Microsoft.AspNetCore.Identity;

namespace PrimeApps.Auth
{
	public class CustomProfileService : IProfileService
	{
		private readonly UserManager<ApplicationUser> _userManager;

		public CustomProfileService(UserManager<ApplicationUser> userManager)
		{
			_userManager = userManager;
		}


		public Task GetProfileDataAsync(ProfileDataRequestContext context)
		{
			var sub = context.Subject.GetSubjectId();

			var user = _userManager.FindByIdAsync(context.Subject.GetSubjectId()).Result;

			/*var claims = new Claim[]
			{
				new Claim("foo", "bar")
			};*/
			context.IssuedClaims.Add(new Claim("email", user.Email));
			context.IssuedClaims.Add(new Claim("email_confirmed", user.EmailConfirmed.ToString()));
			return Task.CompletedTask;
			/*context.AddRequestedClaims(claims);*/
		}

		public Task IsActiveAsync(IsActiveContext context)
		{
			var sub = context.Subject.GetSubjectId();
			var user = _userManager.FindByIdAsync(context.Subject.GetSubjectId()).Result;
			context.IsActive = user != null;
			return Task.FromResult(user != null);
		}
	}
}