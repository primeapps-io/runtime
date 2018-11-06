using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Helpers;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Steps
{
    public class FunctionStep : StepBodyAsync
    {
        private IServiceScopeFactory _serviceScopeFactory;

        public string Request { get; set; }

        public string Response { get; set; }

        public FunctionStep(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            if (context == null)
                throw new NullReferenceException();

            var request = Request != null ? JObject.Parse(Request.Replace("\\", "")) : null;

            if (request == null || request.IsNullOrEmpty())
                throw new Exception("Cannot find Request");

            using (var scope = _serviceScopeFactory.CreateScope())
            {
                var functionHelper = scope.ServiceProvider.GetRequiredService<IFunctionHelper>();
                var functionName = (string)request["function"]["name"];
                var functionHttpMethod = (string)request["function"]["methodType"];
                var functionBody = (string)request["function"]["postBody"];
                var functionUrl = await functionHelper.GetFunctionUrl(functionName);

                if (string.IsNullOrWhiteSpace(functionUrl))
                    throw new Exception("Function not found.");

                var response = await functionHelper.Run(functionUrl, functionHttpMethod, functionBody);
                var result = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                    throw new Exception("Function error occurred. StatusCode: " + response.StatusCode + " Content: " + result);

                Response = result;
            }

            return ExecutionResult.Next();
        }
    }
}
