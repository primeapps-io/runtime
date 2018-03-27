using System.Net.Http;
using Microsoft.AspNetCore.Mvc.Filters;


namespace PrimeApps.App.ActionFilters
{
    public class NoCache : ActionFilterAttribute
    {
        

        private static void SetCacheControl(HttpResponseMessage response)
        {
             
        }
    }
}