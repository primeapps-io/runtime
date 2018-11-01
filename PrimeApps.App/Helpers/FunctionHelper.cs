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
        JObject CreateRequest(FunctionBindingModel model);
        JObject UpdateRequest(FunctionBindingModel model, JObject function);
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

        public JObject CreateRequest(FunctionBindingModel model)
        {
            var function = new JObject();
            function["kind"] = "Function";
            function["apiVersion"] = "kubeless.io/v1beta1";
            function["metadata"] = new JObject();
            function["metadata"]["name"] = model.Name;
            function["metadata"]["namespace"] = "default";
            function["spec"] = new JObject();
            function["spec"]["deps"] = model.Dependencies;
            function["spec"]["function"] = "";
            function["spec"]["checksum"] = model.Name.ToSha256();
            function["spec"]["handler"] = model.Handler;
            function["spec"]["runtime"] = model.Runtime.GetAttributeOfType<EnumMemberAttribute>().Value;

            return function;
        }

        public JObject UpdateRequest(FunctionBindingModel model, JObject function)
        {
            function["spec"]["deps"] = model.Dependencies;
            function["spec"]["function"] = model.Function;
            function["spec"]["handler"] = model.Handler;
            function["spec"]["runtime"] = model.Runtime.GetAttributeOfType<EnumMemberAttribute>().Value;

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