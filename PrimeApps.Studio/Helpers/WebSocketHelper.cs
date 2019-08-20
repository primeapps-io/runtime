using System;
using System.IO;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Devart.Data.PostgreSql;
using Hangfire.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Util.Storage;

namespace PrimeApps.Studio.Helpers
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

                var _appDraftRepository = (IAppDraftRepository)hContext.RequestServices.GetService(typeof(IAppDraftRepository));
                var _tenantRepository = (ITenantRepository)hContext.RequestServices.GetService(typeof(ITenantRepository));

                var appIds = _appDraftRepository.GetAppIdsByOrganizationId(organizationId);
                var previewMode = "";

                if (tenantId != 0)
                {
                    var tenant = _tenantRepository.Get(tenantId);

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
                //var logType = wsParameters["type"].ToString();

                /*if (!string.IsNullOrEmpty(logType))
                {
                    if (logType == "release")
                        await ReleaseLog(wSocket, wsParameters, result, dbName, appId);

                    else if (logType == "distribute")
                        await DistributeLog(wSocket, wsParameters, result, dbName, appId);
                }*/

                var releaseIdResult = int.TryParse(wsParameters["release_id"].ToString(), out var releaseId);

                if (!releaseIdResult)
                {
                    await wSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                    await wSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                    throw new Exception("Release id not found.");
                }

                while (result.MessageType != WebSocketMessageType.Close)
                {
                    string text;
                    Package package;
                    //AppDraft app = null;
                    //JObject releaseOptions;
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var databaseContext = scope.ServiceProvider.GetRequiredService<StudioDBContext>();

                        using (var packageRepository = new PackageRepository(databaseContext, _configuration))
                        using (var appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                        {
                            var path = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

                            package = await packageRepository.Get(releaseId);
                            //releaseOptions = JObject.Parse(package.Settings);

                            /*if (releaseOptions["type"].ToString() == "publish")
                                app = await appDraftRepository.Get(appId);*/

                            using (var fs = new FileStream($"{path}\\releases\\{dbName}\\{package.Version}\\log.txt", FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            using (var sr = new StreamReader(fs, Encoding.Default))
                            {
                                text = ConvertHelper.ASCIIToHTML(sr.ReadToEnd());

                                if (package.Status != ReleaseStatus.Succeed && text.Contains("********** Package Created **********"))
                                {
                                    try
                                    {
                                        package.Status = ReleaseStatus.Succeed;
                                        await packageRepository.Update(package);

                                        var bucketName = UnifiedStorage.GetPath("releases", previewMode, previewMode == "tenant" ? tenantId : appId, package.Version + "/");

                                        var _storage = (IUnifiedStorage)hContext.RequestServices.GetService(typeof(IUnifiedStorage));
                                        await _storage.UploadDirAsync(bucketName, $"{path}releases\\{dbName}\\{package.Version}");
                                        sr.Close();
                                        fs.Close();

                                        Directory.Delete($"{path}releases\\{dbName}\\{package.Version}", true);
                                    }
                                    catch (Exception e)
                                    {
                                        Console.WriteLine(e);
                                        throw;
                                    }
                                }
                                /*if (app != null && app.Status != PublishStatus.Published && text.Contains("********** Publish End**********"))
                                {
                                    /*release.Status = ReleaseStatus.Succeed;
                                    release.Published = true;
                                    await releaseRepository.Update(release);//

                                    release.Status = ReleaseStatus.Succeed;
                                    await releaseRepository.Update(release);

                                    app.Status = PublishStatus.Published;
                                    await appDraftRepository.Update(app);

                                    release.Published = true;
                                    await releaseRepository.Update(release);

                                    var bucketName = UnifiedStorage.GetPath("releases", previewMode, previewMode == "tenant" ? tenantId : appId, "/" + release.Version + "/");

                                    var _storage = (IUnifiedStorage)hContext.RequestServices.GetService(typeof(IUnifiedStorage));
                                    await _storage.UploadDirAsync(bucketName, $"{path}releases\\{dbName}\\{release.Version}");
                                    
                                    
                                    sr.Close();
                                    fs.Close();
                                            
                                    Directory.Delete($"{path}releases\\{dbName}\\{release.Version}", true);
                                    /* 
                                    var bucketName = UnifiedStorage.GetPath("releases", null, appId, "/" + release.Version + "/");
    
                                    var _storage = (IUnifiedStorage)hContext.RequestServices.GetService(typeof(IUnifiedStorage));
                                    await _storage.UploadDirAsync(bucketName, $"{path}\\releases\\{dbName}\\{release.Version}");
                                    //
                                }*/
                                else if (text.Contains("Error"))
                                    await wSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);
                            }
                        }
                    }

                    await wSocket.SendAsync(CreateWSMessage(text), result.MessageType, result.EndOfMessage, CancellationToken.None);

                    //if ((release.Status != ReleaseStatus.Running && releaseOptions["type"].ToString() == "package") || (app != null && app.Status == PublishStatus.Published && releaseOptions["type"].ToString() == "publish"))
                    if (package.Status != ReleaseStatus.Running)
                        await wSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, result.CloseStatusDescription, CancellationToken.None);

                    Thread.Sleep(2000);
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}