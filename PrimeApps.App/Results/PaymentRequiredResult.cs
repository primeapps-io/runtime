using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace PrimeApps.App.Results
{
    public class PaymentRequiredResult : IHttpActionResult
    {
            private readonly HttpRequestMessage _request;
            private readonly string _reason;

            public PaymentRequiredResult(HttpRequestMessage request, string reason)
            {
                _request = request;
                _reason = reason;
            }

            public PaymentRequiredResult(HttpRequestMessage request)
            {
                _request = request;
                _reason = "Payment Required";
            }

            public Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
            {
                var response = _request.CreateResponse(HttpStatusCode.PaymentRequired, _reason);
                return Task.FromResult(response);
            }
        }
}