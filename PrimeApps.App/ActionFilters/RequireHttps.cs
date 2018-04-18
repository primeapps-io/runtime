using System;
using System.Configuration;
using System.Net.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Protocols;

namespace PrimeApps.App.ActionFilters
{
	public class RequireHttps : RequireHttpsAttribute
	{
		public override void OnAuthorization(AuthorizationFilterContext filterContext)
		{
			var allowInsecureHttp = bool.Parse(ConfigurationManager.AppSettings["AllowInsecureHttp"]);

			if (filterContext.HttpContext.Request.Scheme != Uri.UriSchemeHttps && !allowInsecureHttp)
			{
				HandleNonHttpsRequest(filterContext);
			}
			else
			{
				base.OnAuthorization(filterContext);
			}
		}

		protected override void HandleNonHttpsRequest(AuthorizationFilterContext filterContext)
		{
			filterContext.Result = new ForbidResult("SSL Required");
		}
	}
}