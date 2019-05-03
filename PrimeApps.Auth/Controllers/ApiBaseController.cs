using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PrimeApps.Auth.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public abstract class ApiBaseController<T> : Controller where T : ApiBaseController<T>
    {
        private IConfiguration _configuration;
        protected IConfiguration Configuration => _configuration ?? (_configuration = HttpContext.RequestServices.GetService<IConfiguration>());

        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            var email = HttpContext.User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;

            if (string.IsNullOrEmpty(email) || !email.EndsWith("@primeapps.io"))
                ctx.Result = new UnauthorizedResult();

            base.OnActionExecuting(ctx);
        }
    }
}