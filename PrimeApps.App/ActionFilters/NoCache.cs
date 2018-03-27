using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Filters;

namespace PrimeApps.App.ActionFilters
{
    public class NoCache : ActionFilterAttribute
    {
        public override void OnActionExecuted(HttpActionExecutedContext actionExecutedContext)
        {
            SetCacheControl(actionExecutedContext.Response);
            base.OnActionExecuted(actionExecutedContext);
        }

        public override Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            SetCacheControl(actionExecutedContext.Response);
            return base.OnActionExecutedAsync(actionExecutedContext, cancellationToken);
        }

        private static void SetCacheControl(HttpResponseMessage response)
        {
            if (response == null)
                return;

            response.Headers.CacheControl = new CacheControlHeaderValue()
            {
                NoCache = true,
                NoStore = true,
                MaxAge = new TimeSpan(0),
                MustRevalidate = true
            };
        }
    }
}