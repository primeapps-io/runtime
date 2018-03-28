using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
namespace PrimeApps.App.Results
{
    public class MethodNotAllowedResult : IActionResult
    {
        private readonly HttpRequestMessage _request;
        private readonly string _reason;

        public MethodNotAllowedResult(HttpRequestMessage request, string reason)
        {
            _request = request;
            _reason = reason;
        }

        public MethodNotAllowedResult(HttpRequestMessage request)
        {
            _request = request;
            _reason = "Method Not Allowed";
        }

        public Task ExecuteResultAsync(ActionContext context)
        {
            var response = new HttpResponseMessage(HttpStatusCode.Unauthorized);
            return Task.FromResult(response);
        }
    }
}