using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

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

        public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = _request.CreateResponse(HttpStatusCode.MethodNotAllowed, _reason);
                return Task.FromResult(response);
            }
        }
}