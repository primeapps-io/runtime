using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Console.Models;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Console.Helpers
{
    public interface IFunctionHelper
    {
        JObject CreateFunctionRequest(FunctionBindingModel model, JObject functionCurrent = null);
        Task<JObject> Get(string functionName);
        Task<string> GetFunctionUrl(string functionName);
        Task<HttpResponseMessage> Run(string functionUrl, string functionHttpMethod, string functionRequestBody);
        Task<JArray> GetPods(string functionName);
        Task<string> GetLogs(string podName);
    }

    public class FunctionHelper : IFunctionHelper
    {
        private IConfiguration _configuration;
        private string _kubernetesClusterRootUrl;

        public FunctionHelper(IConfiguration configuration)
        {
            _configuration = configuration;
            _kubernetesClusterRootUrl = _configuration["AppSettings:KubernetesClusterRootUrl"];
        }

        public JObject CreateFunctionRequest(FunctionBindingModel model, JObject functionCurrent = null)
        {
            var function = new JObject
            {
                ["kind"] = "Function",
                ["apiVersion"] = "kubeless.io/v1beta1",
                ["metadata"] = new JObject()
            };
            function["metadata"]["name"] = model.Name;
            function["metadata"]["namespace"] = "default";
            function["spec"] = new JObject
            {
                ["function"] = "",
                ["deps"] = !string.IsNullOrWhiteSpace(model.Dependencies) ? model.Dependencies : "",
                ["handler"] = model.Handler,
                ["runtime"] = model.Runtime.GetAttributeOfType<EnumMemberAttribute>().Value,
                ["function-content-type"] = model.ContentType.GetAttributeOfType<EnumMemberAttribute>().Value
            };
            function["timeout"] = "180";

            if (functionCurrent != null && !functionCurrent.IsNullOrEmpty())
            {
                function["metadata"]["resourceVersion"] = (string)functionCurrent["metadata"]["resourceVersion"];
                function["metadata"]["uid"] = (string)functionCurrent["metadata"]["uid"];
                function["metadata"]["generation"] = (int)functionCurrent["metadata"]["generation"];
                function["spec"]["function"] = model.Function;

                if (model.ContentType == FunctionContentType.Text)
                    function["spec"]["checksum"] = "sha256:" + model.Function.ToSha256();
            }

            return function;
        }

        public async Task<JObject> Get(string functionName)
        {
            JObject function;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/namespaces/default/functions/{functionName}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                function = JObject.Parse(content);
            }

            return function;
        }

        public async Task<string> GetFunctionUrl(string functionName)
        {
            string functionUrl;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/api/v1/namespaces/default/services/" + functionName;

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                var function = JObject.Parse(content);
                var port = 8080;

                if (!function["spec"].IsNullOrEmpty() && !function["spec"]["ports"].IsNullOrEmpty())
                    port = (int)((JArray)function["spec"]["ports"])[0]["port"];

                functionUrl = $"{_kubernetesClusterRootUrl}/api/v1/namespaces/default/services/{functionName}:{port}/proxy/";
            }

            return functionUrl;
        }

        public async Task<HttpResponseMessage> Run(string functionUrl, string functionHttpMethod, string functionRequestBody)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;
                var httpMethod = new HttpMethod(functionHttpMethod);

                if (httpMethod == HttpMethod.Get)
                {
                    response = await httpClient.GetAsync(functionUrl);
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    var requestContent = new StringContent(functionRequestBody, Encoding.UTF8, "application/json");
                    response = await httpClient.PostAsync(functionUrl, requestContent);
                }
                else
                {
                    throw new Exception("Unsupported HttpMethod.");
                }

                return response;
            }
        }

        public async Task<JArray> GetPods(string functionName)
        {
            JArray pods;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/api/v1/pods?labelSelector=function%3D{functionName}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                var result = JObject.Parse(content);
                pods = (JArray)result["items"];
            }

            return pods;
        }

        public async Task<string> GetLogs(string podName)
        {
            string logs;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/api/v1/namespaces/default/pods/{podName}/log";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode && response.StatusCode == HttpStatusCode.NotFound)
                    return null;

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                logs = content;
            }

            return logs;
        }
    }
}