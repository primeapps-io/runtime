using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http.Controllers;

namespace PrimeApps.App.Helpers
{
    public class SnakeCaseActionSelector : ApiControllerActionSelector
    {
        public override HttpActionDescriptor SelectAction(HttpControllerContext controllerContext)
        {
            var queryNameValuePairs = controllerContext.Request.GetQueryNameValuePairs();
            var nameValuePairs = queryNameValuePairs as IList<KeyValuePair<string, string>> ?? queryNameValuePairs.ToList();

            if (!nameValuePairs.Any())
                return base.SelectAction(controllerContext);

            var newUri = CreateNewUri(controllerContext.Request.RequestUri, nameValuePairs);
            controllerContext.Request.RequestUri = newUri;

            return base.SelectAction(controllerContext);
        }

        private Uri CreateNewUri(Uri requestUri, IEnumerable<KeyValuePair<string, string>> queryPairs)
        {
            var currentQuery = requestUri.Query;
            var newQuery = ConvertQueryToCamelCase(queryPairs);
            return new Uri(requestUri.ToString().Replace(currentQuery, newQuery));
        }

        private static string ConvertQueryToCamelCase(IEnumerable<KeyValuePair<string, string>> queryPairs)
        {
            queryPairs = queryPairs.Select(x => new KeyValuePair<string, string>(x.Key.ToCamelCase(), x.Value));

            return "?" + queryPairs.Select(x => $"{x.Key}={x.Value}").Aggregate((x, y) => x + "&" + y);
        }
    }
}