using Microsoft.Owin.Security;
using Microsoft.Owin.Security.DataHandler;
using Newtonsoft.Json;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.App.Handlers
{
    /// <summary>
    /// This handler will log request and response data for each request being made to our api.
    /// </summary>
    public class ApiLogHandler : DelegatingHandler
    {
        /// <summary>
        /// We override SendAsync method of http handler to identify and log the incoming requests from tenant and public sources.
        /// </summary>
        /// <param name="request"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var apiLogEntry = CreateEntry(request);

            if (request.Content != null)
            {
                // request has some content/parameters etc. sent so log it.
                await request.Content.ReadAsStringAsync()
                    .ContinueWith(task =>
                    {
                        apiLogEntry.RequestContentBody = task.Result;
                    }, cancellationToken);
            }

            return await base.SendAsync(request, cancellationToken)
                .ContinueWith(task =>
                {
                    //this part will log response data.
                    var response = task.Result;

                    // Update the API log entry with response info
                    apiLogEntry.ResponseStatusCode = (int)response.StatusCode;
                    apiLogEntry.ResponseTimestamp = DateTime.UtcNow;

                    if (response.Content != null)
                    {
                        apiLogEntry.ResponseContentBody = response.Content.ReadAsStringAsync().Result;
                        apiLogEntry.ResponseContentType = response.Content.Headers.ContentType.MediaType;
                        apiLogEntry.ResponseHeaders = SerializeHeaders(response.Content.Headers);
                    }


                    using (var logCtx = new PlatformDBContext())
                    {
                        /// save entry to ofisim database with it's context.
                        //logCtx.ApiLogs.Add(apiLogEntry);
                        logCtx.SaveChanges();
                    }
                    return response;
                }, cancellationToken);
        }

        /// <summary>
        /// Creates an api log entry with incoming http request data.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        private ApiLog CreateEntry(HttpRequestMessage request)
        {
            var context = request.GetOwinContext();
            AuthenticationTicket ticket = null;
            int userId = 0;
            int tenantId = 0;
            string userIdStr = null;
            string tenantIdStr = null;

            /// check if the request is authorized, if it has a header called "Authorization"
            if (context.Request.Headers["Authorization"] != null)
            {
                /// if we have a bearer token, we use our helper MachineKeyProtector class for decoding it.
                var secureDataFormat = new TicketDataFormat(new MachineKeyProtector());
                ticket = secureDataFormat.Unprotect(context.Request.Headers["Authorization"].Replace("Bearer ", ""));
            }

            /// try to get user id and tenant id from undecoded token / ticket.
            userIdStr = ticket?.Identity.Claims.SingleOrDefault(x => x.Type == "user_id")?.Value;
            tenantIdStr = ticket?.Identity.Claims.SingleOrDefault(x => x.Type == "tenant_id")?.Value;
            int.TryParse(userIdStr, out userId);
            int.TryParse(tenantIdStr, out tenantId);

            return new ApiLog
            {
                User = ticket?.Identity?.Name,
                UserId = userId,
                TenantId = tenantId,
                Machine = Environment.MachineName,
                RequestContentType = context.Request.ContentType,
                RequestRoute = context.Request.Path.ToString(),
                RequestIpAddress = context.Request.RemoteIpAddress,
                RequestMethod = request.Method.Method,
                RequestHeaders = SerializeHeaders(request.Headers),
                RequestTimestamp = DateTime.Now,
                RequestUri = request.RequestUri.ToString()
            };
        }

        /// <summary>
        /// Serializes headers as json string.
        /// </summary>
        /// <param name="headers"></param>
        /// <returns></returns>
        private string SerializeHeaders(HttpHeaders headers)
        {
            var dict = new Dictionary<string, string>();

            foreach (var item in headers.ToList())
            {
                if (item.Value != null)
                {
                    var header = String.Empty;
                    foreach (var value in item.Value)
                    {
                        header += value + " ";
                    }

                    // Trim the trailing space and add item to the dictionary
                    header = header.TrimEnd(" ".ToCharArray());
                    dict.Add(item.Key, header);
                }
            }

            return JsonConvert.SerializeObject(dict, Formatting.Indented);
        }
    }
}