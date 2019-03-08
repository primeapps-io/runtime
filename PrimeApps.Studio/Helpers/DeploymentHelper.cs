using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using PrimeApps.Studio.Models;
using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Studio.Storage;

namespace PrimeApps.Studio.Helpers
{
    public interface IDeploymentHelper
    {
        Task StartFunctionDeployment(Model.Entities.Tenant.Function function, JObject functionObj, string name, int userId, int organizationId, int appId, int deploymentId);
        Task StartComponentDeployment(Model.Entities.Tenant.Component component, string giteaToken, string email, int appId, int deploymentId, int? tenantId = null);
        Task StartScriptDeployment(Model.Entities.Tenant.Component script, string giteaToken, string email, int appId, int deploymentId, int? tenantId = null);
    }

    public class DeploymentHelper : IDeploymentHelper
    {
        private string _kubernetesClusterRootUrl;
        private CurrentUser _currentUser;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IUnifiedStorage _storage;
        private IGiteaHelper _giteaHelper;
        private IFunctionHelper _functionHelper;
        private IDocumentHelper _documentHelper;

        public DeploymentHelper(IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            IHttpContextAccessor context,
            IGiteaHelper giteaHelper,
            IUnifiedStorage storage,
            IFunctionHelper functionHelper,
            IDocumentHelper documentHelper)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _context = context;
            _functionHelper = functionHelper;
            _documentHelper = documentHelper;
            _storage = storage;
            _giteaHelper = giteaHelper;
            _currentUser = UserHelper.GetCurrentUser(_context);
            _kubernetesClusterRootUrl = _configuration["AppSettings:KubernetesClusterRootUrl"];
        }

        public async Task StartFunctionDeployment(Model.Entities.Tenant.Function function, JObject functionObj, string name, int userId, int organizationId, int appId, int deploymentId)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                var tenantDBContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _organizationRepository = new OrganizationRepository(databaseContext, _configuration))
                using (var _appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                using (var _deploymentFunctionRepository = new DeploymentFunctionRepository(tenantDBContext, _configuration))
                {
                    _organizationRepository.CurrentUser = _appDraftRepository.CurrentUser = _deploymentFunctionRepository.CurrentUser = _currentUser;

                    var organization = await _organizationRepository.Get(userId, organizationId);
                    var app = await _appDraftRepository.Get((int)appId);
                    var functionType = _functionHelper.GetTypeWithRuntime(function.Runtime);
                    var code = await _giteaHelper.GetFile(function.Name + "." + functionType, organization.Name, app.Name, CustomCodeType.Functions);

                    var functionModel = new FunctionBindingModel
                    {
                        Name = name,
                        Function = code,
                        Handler = function.Handler,
                        Runtime = function.Runtime,
                        Dependencies = function.Dependencies,
                        ContentType = function.ContentType
                    };

                    var functionRequest = _functionHelper.CreateFunctionRequest(functionModel, functionObj);
                    JObject result;

                    using (var httpClient = new HttpClient())
                    {
                        var url = $"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/namespaces/default/functions/{name}";

                        httpClient.DefaultRequestHeaders.Accept.Clear();
                        httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                        var response = await httpClient.PutAsync(url, new StringContent(JsonConvert.SerializeObject(functionRequest), Encoding.UTF8, "application/json"));
                        var content = await response.Content.ReadAsStringAsync();

                        result = JObject.Parse(content);

                        var deployment = await _deploymentFunctionRepository.Get(deploymentId);

                        if (!response.IsSuccessStatusCode)
                            deployment.Status = DeploymentStatus.Failed;
                        else
                            deployment.Status = DeploymentStatus.Succeed;

                        deployment.EndTime = DateTime.Now;

                        await _deploymentFunctionRepository.Update(deployment);
                    }
                }
            }
        }

        public async Task StartComponentDeployment(Model.Entities.Tenant.Component component, string giteaToken, string email, int appId, int deploymentId, int? tenantId = null)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                var tenantDBContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                using (var _deploymentComponentRepository = new DeploymentComponentRepository(tenantDBContext, _configuration))
                using (var _componentRepository = new ComponentRepository(tenantDBContext, _configuration))
                {
                    _appDraftRepository.CurrentUser = _deploymentComponentRepository.CurrentUser = _componentRepository.CurrentUser = _currentUser;

                    var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);

                    if (!string.IsNullOrEmpty(enableGiteaIntegration) && bool.Parse(enableGiteaIntegration))
                    {
                        var app = await _appDraftRepository.Get(appId);
                        var repository = await _giteaHelper.GetRepositoryInfo(giteaToken, email, app.Name);
                        if (repository != null)
                        {
                            var giteaDirectory = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);

                            if (!string.IsNullOrEmpty(giteaDirectory))
                            {
                                var localPath = giteaDirectory + repository["name"].ToString();
                                _giteaHelper.CloneRepository(giteaToken, repository["clone_url"].ToString(), localPath);
                                var files = _giteaHelper.GetFileNames(localPath, "components/" + component.Name);
                                var deployment = await _deploymentComponentRepository.Get(deploymentId);

                                try
                                {
                                    foreach (var file in files)
                                    {
                                        var path = file["path"].ToString();
                                        var pathArray = path.Split("/");
                                        var fileName = pathArray[pathArray.Length - 1];

                                        var code = File.ReadAllText(localPath + "//" + path);

                                        var content = JObject.Parse(component.Content);
                                        var folderName = content.HasValues && content["level"] != null && content["level"].ToString() != "app" ? "tenant-" + tenantId : "app-" + appId;
                                        var bucketName = UnifiedStorage.GetPathComponents(folderName, component.Name);

                                        var stream = new MemoryStream();
                                        var writer = new StreamWriter(stream);
                                        writer.Write(code);
                                        writer.Flush();
                                        stream.Position = 0;

                                        await _storage.Upload(bucketName, fileName, stream);

                                        if (content["app"] != null && content["app"]["templateFile"].ToString() == fileName)
                                        {
                                            var url = _storage.GetShareLink(bucketName, fileName, DateTime.UtcNow.AddYears(100), Amazon.S3.Protocol.HTTP);
                                            content["app"]["templateUrl"] = url;
                                            component.Content = JsonConvert.SerializeObject(content);
                                            await _componentRepository.Update(component);
                                        }
                                    }

                                    deployment.Status = DeploymentStatus.Succeed;
                                }
                                catch (Exception ex)
                                {
                                    deployment.Status = DeploymentStatus.Failed;
                                    ErrorHandler.LogError(ex, "Component deployment error.");
                                }

                                deployment.EndTime = DateTime.Now;
                                await _deploymentComponentRepository.Update(deployment);
                                _giteaHelper.DeleteDirectory(localPath);
                            }
                        }
                    }
                }
            }
        }

        public async Task StartScriptDeployment(Model.Entities.Tenant.Component script, string giteaToken, string email, int appId, int deploymentId, int? tenantId = null)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                var tenantDBContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                using (var _deploymentComponentRepository = new DeploymentComponentRepository(tenantDBContext, _configuration))
                using (var _scriptRepository = new ScriptRepository(tenantDBContext, _configuration))
                {
                    _appDraftRepository.CurrentUser = _deploymentComponentRepository.CurrentUser = _scriptRepository.CurrentUser = _currentUser;

                    var enableGiteaIntegration = _configuration.GetValue("AppSettings:GiteaEnabled", string.Empty);

                    if (!string.IsNullOrEmpty(enableGiteaIntegration) && bool.Parse(enableGiteaIntegration))
                    {
                        var app = await _appDraftRepository.Get(appId);
                        var repository = await _giteaHelper.GetRepositoryInfo(giteaToken, email, app.Name);
                        if (repository != null)
                        {
                            var giteaDirectory = _configuration.GetValue("AppSettings:GiteaDirectory", string.Empty);
                            var code = "";

                            if (!string.IsNullOrEmpty(giteaDirectory))
                            {
                                var localPath = giteaDirectory + repository["name"].ToString();
                                _giteaHelper.CloneRepository(giteaToken, repository["clone_url"].ToString(), localPath);
                                // var files = _giteaHelper.GetFileNames(localPath, "components/" + script.Name);
                                var deployment = await _deploymentComponentRepository.Get(deploymentId);
                                var fileName = $"/scripts/{script.Name}.js";

                                try
                                {
                                    code = File.ReadAllText(localPath + "/" + fileName);
                                    deployment.Status = DeploymentStatus.Succeed;
                                }
                                catch (Exception ex)
                                {
                                    deployment.Status = DeploymentStatus.Failed;
                                    ErrorHandler.LogError(ex, "Script deployment error.");
                                }

                                deployment.EndTime = DateTime.Now;
                                await _deploymentComponentRepository.Update(deployment);

                                var entity = await _scriptRepository.Get(script.Id);
                                entity.Content = code;

                                var result = await _scriptRepository.Update(entity);

                                if (result > 0)
                                    _giteaHelper.DeleteDirectory(localPath);
                            }
                        }
                    }
                }
            }
        }
    }
}