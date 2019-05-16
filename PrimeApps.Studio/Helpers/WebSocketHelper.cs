using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Devart.Data.PostgreSql;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Studio.Helpers
{
    public interface IWebSocketHelper
    {
        Task LogStream(HttpContext hContext, WebSocket wSocket, int deploymentId);
    }

    public class WebSocketHelper : IWebSocketHelper
    {
        private IConfiguration _configuration;

        private string PDEConnectionString;
        private string PREConnectionString;

        public WebSocketHelper(IConfiguration configuration)
        {
            _configuration = configuration;

            PDEConnectionString = _configuration.GetConnectionString("StudioDBConnection");
            PREConnectionString = _configuration.GetConnectionString("PlatformDBConnection");
        }

        public ArraySegment<byte> CreateWSMessage(string message)
        {
            var outgoingMessage = System.Text.Encoding.UTF8.GetBytes(message);
            return new ArraySegment<byte>(outgoingMessage, 0, outgoingMessage.Length);
        }


        public async Task LogStream(HttpContext hContext, WebSocket wSocket, int deploymentId)
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

                var email = hContext.User?.FindFirst("email")?.Value;

                if (string.IsNullOrEmpty(email))
                    throw new Exception("User is not valid.");

                var platformUserRepository = (IPlatformUserRepository)hContext.RequestServices.GetService(typeof(IPlatformUserRepository));
                platformUserRepository.CurrentUser = new CurrentUser {UserId = 1};

                var platformUser = platformUserRepository.GetByEmail(email);

                if (platformUser == null)
                    throw new Exception("User not found.");

                if (string.IsNullOrEmpty(wsParameters["X-Organization-Id"].ToString()) || !int.TryParse(wsParameters["X-Organization-Id"].ToString(), out var organizationId))
                    throw new Exception("Organization not found.");

                var organizationRepository = (IOrganizationRepository)hContext.RequestServices.GetService(typeof(IOrganizationRepository));
                var check = organizationRepository.IsOrganizationAvaliable(platformUser.Id, organizationId);

                if (!check)
                    throw new Exception("Authentication error.");

                int.TryParse(wsParameters["X-App-Id"].ToString(), out appId);

                int.TryParse("X-Tenant-Id", out var tenantId);

                if (tenantId == 0 && appId == 0)
                    throw new Exception("Authentication error.");

                var appDraftRepository = (IAppDraftRepository)hContext.RequestServices.GetService(typeof(IAppDraftRepository));
                var tenantRepository = (ITenantRepository)hContext.RequestServices.GetService(typeof(ITenantRepository));

                var appIds = appDraftRepository.GetAppIdsByOrganizationId(organizationId);
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

                while (!result.CloseStatus.HasValue)
                {
                    var deploymentRepository = (IDeploymentRepository)hContext.RequestServices.GetService(typeof(IDeploymentRepository));
                    var deployment = await deploymentRepository.Get(deploymentId);

                    if (deployment == null || deployment.Status != DeploymentStatus.Running)
                    {
                        result = await wSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        await wSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                        await wSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                        continue;
                        //throw new Exception("Deployment id not found.");
                    }

                    var path = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
                    var text = "";
                    using (var fs = new FileStream($"{path}\\published\\logs\\\\{dbName}\\{deployment.Version}.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    using (var sr = new StreamReader(fs, Encoding.Default))
                    {
                        text = sr.ReadToEnd();
                    }


                    await wSocket.SendAsync(CreateWSMessage(text), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    Thread.Sleep(2000);

                    /*await wSocket.SendAsync(CreateWSMessage("Database sql generating"), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    var sqlDump = PublishHelper.GetSqlDump(PDEConnectionString, Location.PDE, dbName);

                    if (string.IsNullOrEmpty(sqlDump))
                        throw new Exception("Unhandled Exception");

                    await wSocket.SendAsync(CreateWSMessage("Restoring your database"), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    var restoreResult = PublishHelper.RestoreDatabase(PREConnectionString, sqlDump, Location.PRE, dbName);

                    if (!restoreResult)
                        throw new Exception("Unhandled Exception");

                    await wSocket.SendAsync(CreateWSMessage("System tables clearing"), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    PublishHelper.CleanUpSystemTables(PREConnectionString, Location.PRE, dbName);

                    await wSocket.SendAsync(CreateWSMessage("Dynamic tables clearing"), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    JArray tableNames = null;

                    if (wsParameters["table_names"] != null && wsParameters["table_names"].Type == JTokenType.Array)
                        tableNames = JArray.Parse(wsParameters["table_names"].ToString());

                    PublishHelper.CleanUpTables(PREConnectionString, Location.PRE, dbName, tableNames);

                    await wSocket.SendAsync(CreateWSMessage("Records are marking as sample"), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    PublishHelper.SetRecordsIsSample(PREConnectionString, Location.PRE, dbName);

                    await wSocket.SendAsync(CreateWSMessage("Final arrangements being made"), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    PublishHelper.SetAllUserFK(PREConnectionString, Location.PRE, dbName);

                    //await wSocket.SendAsync(CreateWSMessage("Database marking as template"), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    //PublishHelper.SetDatabaseIsTemplate(PREConnectionString, Location.PRE, dbName);

                    await wSocket.SendAsync(CreateWSMessage("You application is ready"), result.MessageType, result.EndOfMessage, CancellationToken.None);
                    //result = await wSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    //await wSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                    
                    await wSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                    await wSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);*/
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}