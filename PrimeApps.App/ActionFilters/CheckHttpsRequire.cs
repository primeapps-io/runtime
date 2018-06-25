using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using System;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.ActionFilters
{
    public class CheckHttpsRequire : RequireHttpsAttribute
    {
        public override void OnAuthorization(AuthorizationFilterContext filterContext)
        {
            if (filterContext == null)
            {
                throw new ArgumentNullException(nameof(filterContext));
            }

            var configuration = (IConfiguration)filterContext.HttpContext.RequestServices.GetService(typeof(IConfiguration));
            var allowInsecureHttp = bool.Parse(configuration.GetSection("AppSettings")["AllowInsecureHttp"]);

            if (!filterContext.HttpContext.Request.IsHttps && !allowInsecureHttp)
            {
                base.HandleNonHttpsRequest(filterContext);
            }
        }

        /// <summary>
        /// Called from <see cref="OnAuthorization"/> if the request is not received over HTTPS. Expectation is
        /// <see cref="AuthorizationFilterContext.Result"/> will not be <c>null</c> after this method returns.
        /// </summary>
        /// <param name="filterContext">The <see cref="AuthorizationFilterContext"/> to update.</param>
        /// <remarks>
        /// If it was a GET request, default implementation sets <see cref="AuthorizationFilterContext.Result"/> to a
        /// result which will redirect the client to the HTTPS version of the request URI. Otherwise, default
        /// implementation sets <see cref="AuthorizationFilterContext.Result"/> to a result which will set the status
        /// code to <c>403</c> (Forbidden).
        /// </remarks>
        protected override void HandleNonHttpsRequest(AuthorizationFilterContext filterContext)
        {
            // only redirect for GET requests, otherwise the browser might not propagate the verb and request
            // body correctly.
            if (!string.Equals(filterContext.HttpContext.Request.Method, "GET", StringComparison.OrdinalIgnoreCase))
            {
                filterContext.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
            }
            else
            {
                var optionsAccessor = filterContext.HttpContext.RequestServices.GetRequiredService<IOptions<MvcOptions>>();

                var request = filterContext.HttpContext.Request;

                var host = request.Host;
                if (optionsAccessor.Value.SslPort.HasValue && optionsAccessor.Value.SslPort > 0)
                {
                    // a specific SSL port is specified
                    host = new HostString(host.Host, optionsAccessor.Value.SslPort.Value);
                }
                else
                {
                    // clear the port
                    host = new HostString(host.Host);
                }

                var newUrl = string.Concat(
                    "https://",
                    host.ToUriComponent(),
                    request.PathBase.ToUriComponent(),
                    request.Path.ToUriComponent(),
                    request.QueryString.ToUriComponent());

                // redirect to HTTPS version of page
                filterContext.Result = new RedirectResult(newUrl, permanent: false);
            }
        }
    }
}
