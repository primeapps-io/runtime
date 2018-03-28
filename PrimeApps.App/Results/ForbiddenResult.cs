using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PrimeApps.App.Results
{
    public class ForbiddenResult : IActionResult
    {
        private readonly HttpRequestMessage _request;
        private readonly string _reason;

        public ForbiddenResult(HttpRequestMessage request, string reason)
        {
            _request = request;
            _reason = reason;
        }

        public ForbiddenResult(HttpRequestMessage request)
        {
            _request = request;
            _reason = "Forbidden";
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            return Task.FromResult(response);
        }
    }
}