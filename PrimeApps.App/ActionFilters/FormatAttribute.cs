using System;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading;
using System.Threading.Tasks;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json;
using ActionFilterAttribute = System.Web.Http.Filters.ActionFilterAttribute;

namespace PrimeApps.App.ActionFilters
{
    public class FormatAttribute : ActionFilterAttribute
    {
        public Task<HttpResponseMessage> ExecuteActionFilterAsync(HttpActionContext actionContext, CancellationToken cancellationToken, Func<Task<HttpResponseMessage>> continuation)
        {
            var formatter = new JsonMediaTypeFormatter
            {
                SerializerSettings =
                {
                    PreserveReferencesHandling = PreserveReferencesHandling.None,
                    ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                    NullValueHandling = NullValueHandling.Ignore,
                    DateParseHandling = DateParseHandling.None,
                    DateTimeZoneHandling = DateTimeZoneHandling.Utc
                }
            };

            actionContext.RequestContext.Configuration.Formatters.Clear();
            actionContext.RequestContext.Configuration.Formatters.Add(formatter);

            return continuation();
        }
    }
}