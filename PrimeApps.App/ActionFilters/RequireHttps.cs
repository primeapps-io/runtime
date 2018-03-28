using System;
using System.Configuration;
using System.Net.Http;
using Microsoft.IdentityModel.Protocols;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
namespace PrimeApps.App.ActionFilters
{
    public class RequireHttpsAttribute : AuthorizationFilterAttribute
    {
        public override void OnAuthorization(HttpActionContext actionContext)
        {
            var allowInsecureHttp = bool.Parse(ConfigurationManager<>.AppSettings["AllowInsecureHttp"]);

            if (actionContext.Request.RequestUri.Scheme != Uri.UriSchemeHttps && !allowInsecureHttp)
            {
                HandleNonHttpsRequest(actionContext);
            }
            else
            {
                base.OnAuthorization(actionContext);
            }
        }

        protected virtual void HandleNonHttpsRequest(HttpActionContext actionContext)
        {
            actionContext.Response = new HttpResponseMessage(System.Net.HttpStatusCode.Status403Forbidden);
            actionContext.Response.ReasonPhrase = "SSL Required";
        }
    }
}