using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Auth.Helpers;

namespace PrimeApps.Auth.Controllers
{
    [Authorize(AuthenticationSchemes = "Bearer")]
    public abstract class ApiBaseController<T> : Controller where T : ApiBaseController<T>
    {
        private IConfiguration _configuration;
        protected IConfiguration Configuration => _configuration ?? (_configuration = HttpContext.RequestServices.GetService<IConfiguration>());

        public override void OnActionExecuting(ActionExecutingContext ctx)
        {
            /*if (!ctx.HttpContext.Request.IsLocal())
                ctx.Result = new UnauthorizedResult();*/

            base.OnActionExecuting(ctx);
        }
    }
}