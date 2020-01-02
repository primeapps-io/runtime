using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace PrimeApps.Studio.Services
{
    public static class ODataQueryStringFixerExtensions
    {
        public static IApplicationBuilder UseODataQueryStringFixer(this IApplicationBuilder app)
        {
            return app.UseMiddleware<ODataQueryStringFixer>();
        }
    }

    /// <summary>
    /// OData kullanan Kendo Grid uzerindeki filter yapisi icin eklendi. Buyuk/kucuk harf duyarliligi icin gelen query'deki filter'i sreplace etmekte.
    /// </summary>
    public class ODataQueryStringFixer : IMiddleware
    {
        private static readonly Regex ReplaceToLowerRegex =
            new Regex(@"\(tolower\((?<columnName>([^\)\(]+))\)%2C(?<value>(\'|%27)(\w+)(\'|%27))\)");

        public Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            var input = context.Request.QueryString.Value;
            if (string.IsNullOrEmpty(input))
                return next(context);

            var replacement = @"(tolower(${columnName}),tolower(${value}))";
            context.Request.QueryString = new QueryString(ReplaceToLowerRegex.Replace(input, replacement));

            return next(context);
        }
    }
}
