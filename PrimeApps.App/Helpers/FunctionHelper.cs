using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Models;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Helpers
{
    public interface IFunctionHelper
    {
        JObject CreateFunctionRequest(FunctionBindingModel model, JObject functionCurrent = null);
        Task<JObject> Get(string name);
        Task<string> GetFunctionUrl(string name);
        Task<HttpResponseMessage> Run(string functionUrl, string functionHttpMethod, string functionRequestBody);
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
            var function = new JObject();
            function["kind"] = "Function";
            function["apiVersion"] = "kubeless.io/v1beta1";
            function["metadata"] = new JObject();
            function["metadata"]["name"] = model.Name;
            function["metadata"]["namespace"] = "default";
            function["spec"] = new JObject();
            function["spec"]["checksum"] = "sha256:" + model.Name.ToSha256();
            function["spec"]["function"] = "";
            function["spec"]["deps"] = model.Dependencies;
            function["spec"]["handler"] = model.Handler;
            function["spec"]["runtime"] = model.Runtime.GetAttributeOfType<EnumMemberAttribute>().Value;

            if (!functionCurrent.IsNullOrEmpty())
            {
                var finalizers = new JArray();
                finalizers.Add("kubeless.io/function");

                function["metadata"]["resourceVersion"] = (string)functionCurrent["metadata"]["resourceVersion"];
                function["metadata"]["uid"] = (string)functionCurrent["metadata"]["uid"];
                function["metadata"]["generation"] = (int)functionCurrent["metadata"]["generation"];
                function["metadata"]["finalizers"] = finalizers;
                function["spec"]["checksum"] = "sha256:" + model.Function.ToSha256();
                function["spec"]["function"] = model.Function;
                function["spec"]["function-content-type"] = "";
                function["spec"]["deployment"] = new JObject();
                function["spec"]["deployment"]["metadata"] = new JObject();
                function["spec"]["deployment"]["metadata"]["creationTimestamp"] = null;
                function["spec"]["deployment"]["spec"] = new JObject();
                function["spec"]["deployment"]["spec"]["strategy"] = new JObject();
                function["spec"]["deployment"]["spec"]["template"] = new JObject();
                function["spec"]["deployment"]["spec"]["template"]["metadata"] = new JObject();
                function["spec"]["deployment"]["spec"]["template"]["metadata"]["creationTimestamp"] = null;
                function["spec"]["deployment"]["spec"]["template"]["spec"] = new JObject();
                function["spec"]["deployment"]["spec"]["template"]["spec"]["containers"] = null;
                function["spec"]["deployment"]["status"] = new JObject();
                function["spec"]["horizontalPodAutoscaler"] = new JObject();
                function["spec"]["horizontalPodAutoscaler"]["metadata"] = new JObject();
                function["spec"]["horizontalPodAutoscaler"]["metadata"]["creationTimestamp"] = null;
                function["spec"]["horizontalPodAutoscaler"]["spec"] = new JObject();
                function["spec"]["horizontalPodAutoscaler"]["spec"]["maxReplicas"] = 0;
                function["spec"]["horizontalPodAutoscaler"]["spec"]["scaleTargetRef"] = new JObject();
                function["spec"]["horizontalPodAutoscaler"]["spec"]["scaleTargetRef"]["kind"] = "";
                function["spec"]["horizontalPodAutoscaler"]["spec"]["scaleTargetRef"]["name"] = "";
                function["spec"]["horizontalPodAutoscaler"]["status"] = new JObject();
                function["spec"]["horizontalPodAutoscaler"]["status"]["conditions"] = null;
                function["spec"]["horizontalPodAutoscaler"]["status"]["currentMetrics"] = null;
                function["spec"]["horizontalPodAutoscaler"]["status"]["currentReplicas"] = 0;
                function["spec"]["horizontalPodAutoscaler"]["status"]["desiredReplicas"] = 0;
                function["service"] = new JObject();
                function["timeout"] = "";
            }

            return function;
        }

        public async Task<JObject> Get(string name)
        {
            JObject function;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/namespaces/default/functions/{name}";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                function = JObject.Parse(content);
            }

            return function;
        }

        public async Task<string> GetFunctionUrl(string name)
        {
            string functionUrl;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/api/v1/namespaces/default/services/" + name;

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                var responseService = await httpClient.GetAsync(url);
                var content = await responseService.Content.ReadAsStringAsync();

                if (!responseService.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + responseService.StatusCode + " Content: " + content);

                var function = JObject.Parse(content);
                var port = 8080;

                if (!function["spec"].IsNullOrEmpty() && !function["spec"]["ports"].IsNullOrEmpty())
                    port = (int)((JArray)function["spec"]["ports"])[0]["port"];

                functionUrl = $"{_kubernetesClusterRootUrl}/api/v1/namespaces/default/services/{name}:{port}/proxy/";
            }

            return functionUrl;
        }

        public async Task<HttpResponseMessage> Run(string functionUrl, string functionHttpMethod, string functionRequestBody)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

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
    }
}