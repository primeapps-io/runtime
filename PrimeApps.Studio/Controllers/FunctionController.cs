using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/functions")]
    public class FunctionController : DraftBaseController
    {
        private IFunctionHelper _functionHelper;
        private IConfiguration _configuration;
        private IFunctionRepository _functionRepository;
        private IGiteaHelper _giteaHelper;
        private IAppDraftRepository _appDraftRepository;
        private IOrganizationRepository _organizationRepository;
        private IDeploymentFunctionRepository _deploymentFunctionRepository;
        private string _kubernetesClusterRootUrl;

        public FunctionController(IConfiguration configuration,
            IFunctionHelper functionHelper,
            IFunctionRepository functionRepository,
            IGiteaHelper giteaHelper,
            IAppDraftRepository appDraftRepository,
            IOrganizationRepository organizationRepository,
            IDeploymentFunctionRepository deploymentFunctionRepository)
        {
            _functionHelper = functionHelper;
            _configuration = configuration;
            _functionRepository = functionRepository;
            _giteaHelper = giteaHelper;
            _appDraftRepository = appDraftRepository;
            _organizationRepository = organizationRepository;
            _deploymentFunctionRepository = deploymentFunctionRepository;
            _kubernetesClusterRootUrl = _configuration["AppSettings:KubernetesClusterRootUrl"];
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_functionRepository, PreviewMode, AppId, TenantId);
            SetCurrentUser(_deploymentFunctionRepository, PreviewMode, AppId, TenantId);
            base.OnActionExecuting(context);
        }

        [Route("count"), HttpGet]
        public async Task<IActionResult> Count()
        {
            var count = await _functionRepository.Count();

            return Ok(count);
        }

        [Route("find"), HttpPost]
        public async Task<IActionResult> Find([FromBody]PaginationModel paginationModel)
        {
            var components = await _functionRepository.Find(paginationModel); ;

            return Ok(components);
        }

        [Route("get/{id}"), HttpGet]
        public async Task<IActionResult> Get(int id)
        {
            var function = await _functionRepository.Get(id);

            if (function == null)
                return BadRequest();

            return Ok(function);
        }

        [Route("get_by_name/{name}"), HttpGet]
        public async Task<IActionResult> Get(string name)
        {
            var function = await _functionRepository.Get(name);

            if (function == null)
                return BadRequest();

            return Ok(function);
        }

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            JArray functions;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/functions";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                var result = JObject.Parse(content);
                functions = (JArray)result["items"];
            }

            return Ok(functions);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]FunctionBindingModel function)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var functionObj = new Function()
            {
                Name = function.Name,
                Label = function.Label,
                Dependencies = function.Dependencies,
                Content = function.ContentType == FunctionContentType.Text ? function.Function : "",
                ContentType = function.ContentType,
                Runtime = function.Runtime,
                Handler = function.Handler,
                Status = PublishStatus.Draft
            };

            var createResult = await _functionRepository.Create(functionObj);

            if (createResult < 0)
                return BadRequest("An error occurred while creating an function");

            var functionRequest = _functionHelper.CreateFunctionRequest(function);
            JObject result;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/namespaces/default/functions";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.PostAsync(url, new StringContent(JsonConvert.SerializeObject(functionRequest), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();
                result = JObject.Parse(content);

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.Conflict)
                    return Conflict(result);

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);
            }

            var giteaEnable = _configuration.GetSection("AppSettings")["EnableGiteaIntegration"];

            if (!string.IsNullOrEmpty(giteaEnable) && bool.Parse(giteaEnable))
            {
                var app = await _appDraftRepository.Get((int)AppId);
                var repository = await _giteaHelper.GetRepositoryInfo(Request.Cookies["gitea_token"], AppUser.Email, app.Name);

                if (repository != null)
                {
                    var localPath = _configuration.GetSection("AppSettings")["GiteaDirectory"] + repository["name"].ToString();
                    _giteaHelper.CloneRepository(Request.Cookies["gitea_token"], repository["clone_url"].ToString(), localPath);

                    var fileName = string.Format("functions/{0}.{1}", function.Name, "cs");

                    if (!System.IO.File.Exists(fileName))
                    {
                        using (var repo = new Repository(localPath))
                        {
                            string sample = string.Format(@"using System;" +
                                "using Kubeless.Functions;" + Environment.NewLine +
                                "using Newtonsoft.Json.Linq;" + Environment.NewLine +
                                "public class {0}{{" + Environment.NewLine +
                                "\tpublic object {0}(Event k8Event, Context k8Context)" + Environment.NewLine +
                                "\t{{" + Environment.NewLine +
                                "\t\tvar obj = new JObject();" + Environment.NewLine +
                                "\t\tobj[\"data\"] = k8Event.Data.ToString();" + Environment.NewLine +
                                "\t\treturn obj;" + Environment.NewLine +
                                "\t}}" + Environment.NewLine +
                                "}}", function.Handler);
                            using (FileStream fs = System.IO.File.Create(localPath + "/" + fileName))
                            {
                                Byte[] info = new UTF8Encoding(true).GetBytes(sample);
                                // Add some information to the file.
                                fs.Write(info, 0, info.Length);
                            }
                            //System.IO.File.WriteAllText(localPath, sample);
                            Commands.Stage(repo, "*");

                            var signature = new Signature(
                                    new Identity("system", "system@primeapps.io"), DateTimeOffset.Now);

                            // Commit to the repository
                            Commit commit = repo.Commit("Created function " + function.Name, signature, signature);
                            _giteaHelper.Push(repo, Request.Cookies["gitea_token"]);

                            repo.Dispose();
                            _giteaHelper.DeleteDirectory(localPath);
                        }
                    }
                }
            }


            return Ok(functionObj.Id);
        }

        [Route("update/{name}"), HttpPut]
        public async Task<IActionResult> Update(string name, [FromBody]FunctionBindingModel function)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var func = await _functionRepository.Get(function.Name);

            if (func == null)
                return BadRequest("Function not found.");

            func.Name = function.Name;
            func.Label = function.Label;
            func.Dependencies = function.Dependencies;
            func.Content = function.ContentType == FunctionContentType.Text ? function.Function : "";
            func.ContentType = function.ContentType;
            func.Runtime = function.Runtime;
            func.Handler = function.Handler;
            func.Status = function.Status;

            var updateResult = await _functionRepository.Update(func);

            if (updateResult < 0)
                return BadRequest("An error occurred while update function");

            var functionObj = await _functionHelper.Get(name);

            if (functionObj.IsNullOrEmpty())
                return NotFound();

            var functionRequest = _functionHelper.CreateFunctionRequest(function, functionObj);
            JObject result;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/namespaces/default/functions/{name}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.PutAsync(url, new StringContent(JsonConvert.SerializeObject(functionRequest), Encoding.UTF8, "application/json"));
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                result = JObject.Parse(content);
            }

            return Ok(result);
        }

        [Route("delete/{name}"), HttpDelete]
        public async Task<IActionResult> Delete(string name)
        {
            var functionObj = await _functionHelper.Get(name);

            if (functionObj.IsNullOrEmpty())
                return NotFound();

            var function = await _functionRepository.Get(name);

            if (function == null)
                return BadRequest();

            var deleteResult = await _functionRepository.Delete(function);

            if (deleteResult < 0)
                return BadRequest("An error occurred while deleting an function");

            JObject result;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/namespaces/default/functions/{name}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.DeleteAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                    return NotFound();

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                result = JObject.Parse(content);
            }

            return Ok(result);
        }

        [Route("run/{name}"), AcceptVerbs("GET", "POST")]
        public async Task<HttpResponseMessage> Run(string name)
        {
            var functionUrl = await _functionHelper.GetFunctionUrl(name);

            if (string.IsNullOrWhiteSpace(functionUrl))
                return new HttpResponseMessage(HttpStatusCode.NotFound);

            string requestBody;

            using (var reader = new StreamReader(Request.Body))
            {
                requestBody = reader.ReadToEnd();
            }

            var response = await _functionHelper.Run(functionUrl, Request.Method, requestBody);

            return response;
        }

        [Route("get_pods/{name}"), HttpGet]
        public async Task<IActionResult> GetPods(string name)
        {
            var pods = await _functionHelper.GetPods(name);

            return Ok(pods);
        }

        [Route("get_logs/{podName}"), HttpGet]
        public async Task<IActionResult> GetLogs(string podName)
        {
            var logs = await _functionHelper.GetLogs(podName);
            var response = new HttpResponseMessage();

            if (logs == null)
                return BadRequest();

            if (logs.Contains("code\":400"))
            {
                var result = JObject.Parse(logs);
                return BadRequest(result["message"].ToString());
            }

            logs = ConvertHelper.ASCIIToHTML(logs);

            return Ok(logs);
        }

        [Route("is_unique_name"), HttpGet]
        public async Task<IActionResult> IsUniqueName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return BadRequest(ModelState);

            var result = await _functionRepository.IsFunctionNameAvailable(name);

            return Ok(result);
        }

        [Route("deploy/{name}"), HttpGet]
        public async Task<IActionResult> Deploy(string name)
        {
            var function = await _functionRepository.Get(name);

            if (function == null)
                return NotFound("Function is not found.");

            var functionObj = await _functionHelper.Get(name);

            if (functionObj.IsNullOrEmpty())
                return NotFound();

            var deployment = new DeploymentFunction
            {
                FunctionId = function.Id,
                Status = DeploymentStatus.Running,
                Version = "12",
                StartTime = DateTime.Now
            };

            var result = await _deploymentFunctionRepository.Create(deployment);

            if (result < 1)
                return BadRequest("Unhandled Exception");

            return Ok();
        }
    }
}
