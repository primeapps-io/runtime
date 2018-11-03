using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Controllers
{
    [Route("api/functions"), Authorize]
    public class FunctionController : ApiBaseController
    {
        private IFunctionHelper _functionHelper;
        private IConfiguration _configuration;
        private string _kubernetesClusterRootUrl;

        public FunctionController(IFunctionHelper functionHelper, IConfiguration configuration)
        {
            _functionHelper = functionHelper;
            _configuration = configuration;
            _kubernetesClusterRootUrl = _configuration["AppSettings:KubernetesClusterRootUrl"];
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);

            base.OnActionExecuting(context);
        }

        [Route("get/{name}"), HttpGet]
        public async Task<IActionResult> Get(string name)
        {
            var function = await _functionHelper.Get(name);

            if (function.IsNullOrEmpty())
                return NotFound();

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

            return Created($"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/namespaces/default/functions/{function.Name}", result);
        }

        [Route("update/{name}"), HttpPut]
        public async Task<IActionResult> Update(string name, [FromBody]FunctionBindingModel function)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

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

            if (logs == null)
                return NotFound();

            return Ok(logs);
        }
    }
}