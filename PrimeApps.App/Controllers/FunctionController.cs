using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;

namespace PrimeApps.App.Controllers
{
    [Route("api/functions")]
    [Authorize]

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

        [Route("get_all"), HttpGet]
        public async Task<IActionResult> GetAll()
        {
            JArray functions;

            using (var httpClient = new HttpClient())
            {
                var url = $"{_kubernetesClusterRootUrl}/apis/kubeless.io/v1beta1/functions";

                httpClient.DefaultRequestHeaders.Accept.Clear();
                httpClient.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));
                var response = await httpClient.GetAsync(url);
                var content = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode || string.IsNullOrWhiteSpace(content))
                    throw new Exception("Kubernetes error. StatusCode: " + response.StatusCode + " Content: " + content);

                var result = JObject.Parse(content);
                functions = (JArray)result["items"];
            }

            return Ok(functions);
        }

        [Route("run/{name}"), AcceptVerbs("GET", "POST")]
        public async Task<HttpResponseMessage> Run(string name)
        {
            var functionUrl = await _functionHelper.GetFunctionUrl(name);

            if (string.IsNullOrWhiteSpace(functionUrl))
                return new HttpResponseMessage(System.Net.HttpStatusCode.NotFound);

            var httpMethod = new HttpMethod(Request.Method);
            var response = await _functionHelper.Run(functionUrl, httpMethod, Request.Body);

            return response;
        }
    }
}