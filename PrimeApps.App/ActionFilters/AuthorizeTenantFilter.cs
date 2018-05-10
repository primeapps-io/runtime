using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Security.Claims;

namespace PrimeApps.App.ActionFilters
{
    public class AuthorizeTenant : ActionFilterAttribute
    {
		public override void OnActionExecuted(ActionExecutedContext context)
		{
			base.OnActionExecuted(context);
		}

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			if (!context.HttpContext.Request.Headers.TryGetValue("X-Tenant-Id", out var tenantIdValues))
				context.Result = new UnauthorizedResult();

			var tenantId = 0;

			if (string.IsNullOrWhiteSpace(tenantIdValues[0]) || !int.TryParse(tenantIdValues[0], out tenantId))
				context.Result = new UnauthorizedResult();

			if (tenantId < 1)
				context.Result = new UnauthorizedResult();

			if (!context.HttpContext.User.Identity.IsAuthenticated || string.IsNullOrWhiteSpace(context.HttpContext.User.FindFirst("email").Value))
				context.Result = new UnauthorizedResult();

			var platformUserRepository = (IPlatformUserRepository)context.HttpContext.RequestServices.GetService(typeof(IPlatformUserRepository));
			var platformUser = platformUserRepository.GetByEmailAndTenantId(context.HttpContext.User.FindFirst("email").Value, tenantId);

			if (platformUser == null || platformUser.TenantsAsUser == null || platformUser.TenantsAsUser.Count < 1)
				context.Result = new UnauthorizedResult();

			context.HttpContext.Items.Add("user", platformUser);

			base.OnActionExecuting(context);
		}
	}
}
