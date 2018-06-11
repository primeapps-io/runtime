using IdentityServer4.Models;
using IdentityServer4.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using PrimeApps.Auth.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace PrimeApps.Auth
{
	public class CustomRedirectUriValidator : IRedirectUriValidator
	{
		private readonly UserManager<ApplicationUser> _userManager;
		private readonly IHttpContextAccessor _context;
		public CustomRedirectUriValidator(UserManager<ApplicationUser> userManager, IHttpContextAccessor context)
		{
			_userManager = userManager;
			_context = context;
		}

		public Task<bool> IsPostLogoutRedirectUriValidAsync(string requestedUri, Client client)
		{
			return Task.FromResult(true);
			throw new NotImplementedException();
		}

		public async Task<bool> IsRedirectUriValidAsync(string requestedUri, Client client)
		{
			if (_context.HttpContext.User.Identity.IsAuthenticated)
			{
				var url = new Uri(requestedUri);

				if (string.IsNullOrEmpty(requestedUri))
					throw new NotImplementedException();

				var apiUrl = url.Scheme + "://" + url.Authority + "/api/platform/get_domain_info?domain=" + url.Authority;

				using (var httpClient = new HttpClient())
				{
					httpClient.BaseAddress = new Uri(apiUrl);
					httpClient.DefaultRequestHeaders.Accept.Clear();
					httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

					var response = await httpClient.GetAsync(requestedUri);

					if (response.IsSuccessStatusCode)
					{

					}
				}

			}

			var a = _context.HttpContext.Request.Host;

			return await Task.FromResult(true);
		}
	}
}
