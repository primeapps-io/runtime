using System;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Admin.Helpers
{
    public interface IWebSocketHelper
    {
        Task LogStream(HttpContext hContext, WebSocket wSocket);
    }

    public class WebSocketHelper : IWebSocketHelper
    {
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;

        public WebSocketHelper(IConfiguration configuration, IServiceScopeFactory serviceScopeFactory)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public ArraySegment<byte> CreateWSMessage(string message)
        {
            var outgoingMessage = System.Text.Encoding.UTF8.GetBytes(message);
            return new ArraySegment<byte>(outgoingMessage, 0, outgoingMessage.Length);
        }

        public async Task LogStream(HttpContext hContext, WebSocket wSocket)
        {
            try
            {
                var appId = 0;

                var buffer = new byte[1024 * 4];
                var result = await wSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                var incomingMessage = System.Text.Encoding.UTF8.GetString(buffer, 0, result.Count);

                if (string.IsNullOrEmpty(incomingMessage))
                    throw new Exception("WebSocket parameters is not valid.");

                var wsParameters = JObject.Parse(incomingMessage);

                // TODO: StudioDbContext admin app'inde olmayacagi icin buraya cozum bulunmali
                var email = hContext.User?.FindFirst("email")?.Value;

                if (string.IsNullOrEmpty(email))
                    throw new Exception("User is not valid.");

                if (string.IsNullOrEmpty(wsParameters["X-Organization-Id"].ToString()) || !int.TryParse(wsParameters["X-Organization-Id"].ToString(), out var organizationId))
                    throw new Exception("Organization not found.");

                var organizationHelper = (IOrganizationHelper)hContext.RequestServices.GetService(typeof(IOrganizationHelper));
                var token = await hContext.GetTokenAsync("access_token");

                var organization = await organizationHelper.GetById(organizationId, token);

                if (organization == null)
                    throw new Exception("Authentication error.");

                int.TryParse(wsParameters["X-App-Id"].ToString(), out appId);
                int.TryParse("X-Tenant-Id", out var tenantId);

                if (tenantId == 0 && appId == 0)
                    throw new Exception("Authentication error.");

                //var _appDraftRepository = (IAppDraftRepository)hContext.RequestServices.GetService(typeof(IAppDraftRepository));
                var tenantRepository = (ITenantRepository)hContext.RequestServices.GetService(typeof(ITenantRepository));

                var appIds = organization.Apps.Select(app => app.Id).ToList();

                var previewMode = "";

                if (tenantId != 0)
                {
                    var tenant = tenantRepository.Get(tenantId);

                    if (!appIds.Contains(tenant.AppId))
                        throw new Exception("Authentication error.");

                    previewMode = "tenant";
                }
                else
                {
                    if (!appIds.Contains(appId))
                        throw new Exception("Authentication error.");

                    previewMode = "app";
                }

                var dbName = previewMode + (previewMode == "tenant" ? tenantId : appId);


                var releaseIdResult = int.TryParse(wsParameters["release_id"].ToString(), out var releaseId);

                if (!releaseIdResult)
                {
                    await wSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                    await wSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                    throw new Exception("Release id not found.");
                }

                while (result.MessageType != WebSocketMessageType.Close)
                {
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var databaseContext = scope.ServiceProvider.GetRequiredService<PlatformDBContext>();

                        using (var releaseRepository = new ReleaseRepository(databaseContext, _configuration))
                        {
                            if (releaseId != 0)
                            {
                                var appProcessActive = await releaseRepository.Get(releaseId);

                                if (appProcessActive != null && appProcessActive.Status != ReleaseStatus.Running)
                                {
                                    var processActive = await releaseRepository.IsThereRunningProcess(appId);

                                    if (!processActive)
                                        await wSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                                }
                            }
                            else
                            {
                                var processActive = await releaseRepository.IsThereRunningProcess(appId);
                                if (!processActive)
                                    await wSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                            }
                        }
                    }

                    await wSocket.SendAsync(CreateWSMessage("There is active process."), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    Thread.Sleep(2000);
                }
            }
            catch (Exception)
            {
            }
        }
    }
}