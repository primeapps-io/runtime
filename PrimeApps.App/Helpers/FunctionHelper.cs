using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Helpers;

namespace PrimeApps.App.Helpers
{
    public interface IFunctionHelper
    {
        Task<string> GetFunctionUrl(string name);
        Task<HttpResponseMessage> Run(string functionUrl, HttpMethod httpMethod, Stream requestBody);
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

        public async Task<HttpResponseMessage> Run(string functionUrl, HttpMethod httpMethod, Stream requestBody)
        {
            using (var httpClient = new HttpClient())
            {
                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

                HttpResponseMessage response;

                if (httpMethod == HttpMethod.Get)
                {
                    response = await httpClient.GetAsync(functionUrl);
                }
                else if (httpMethod == HttpMethod.Post)
                {
                    string body;

                    using (var reader = new StreamReader(requestBody))
                    {
                        body = reader.ReadToEnd();
                    }

                    var requestContent = new StringContent(body, Encoding.UTF8, "application/json");
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