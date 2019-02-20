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
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Studio.Helpers
{
    public interface IDeploymentHelper
    {

    }

    public class DeploymentHelper : IDeploymentHelper
    {
        private string _kubernetesClusterRootUrl;
        private CurrentUser _currentUser;
        private IConfiguration _configuration;
        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;

        private IGiteaHelper _giteaHelper;
        private IFunctionHelper _functionHelper;

        public DeploymentHelper(IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory,
            IHttpContextAccessor context,
            IGiteaHelper giteaHelper,
            IFunctionHelper functionHelper)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _context = context;
            _functionHelper = functionHelper;
            _giteaHelper = giteaHelper;
            _currentUser = UserHelper.GetCurrentUser(_context);
            _kubernetesClusterRootUrl = _configuration["AppSettings:KubernetesClusterRootUrl"];
        }

        public async void StartFunctionDeployment(Function function, JObject functionObj, string name, int userId, int organizationId, int appId)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<ConsoleDBContext>();
                var tenantDBContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();

                using (var _organizationRepository = new OrganizationRepository(databaseContext, _configuration))
                using (var _appDraftRepository = new AppDraftRepository(databaseContext, _configuration))
                using (var _deploymentFunctionRepository = new DeploymentFunctionRepository(tenantDBContext, _configuration))
                {
                    _organizationRepository.CurrentUser = _appDraftRepository.CurrentUser = _currentUser;

                    var organization = await _organizationRepository.Get(userId, organizationId);
                    var app = await _appDraftRepository.Get((int)appId);
                    var functionType = _functionHelper.GetTypeWithRuntime(function.Runtime);
                    var code = await _giteaHelper.GetFile(function.Name + "." + functionType, organization.Name, app.Name, CustomCodeType.Functions);

                    var functionModel = new FunctionBindingModel
                    {
                        Name = name,
                        Function = code
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

                        if (!response.IsSuccessStatusCode)
                        {

                        }

                    }
                }
            }
        }
    }
}
