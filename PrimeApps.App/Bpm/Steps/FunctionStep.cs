using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Helpers;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Steps
{
    public class FunctionStep : StepBodyAsync
    {
        private IFunctionHelper _functionHelper;

        public string Request { get; set; }

        public string Response { get; set; }

        public FunctionStep(IFunctionHelper functionHelper)
        {
            _functionHelper = functionHelper;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            if (context == null)
                throw new NullReferenceException();

            if (context.Workflow.Reference == null)
                throw new NullReferenceException();

            var request = Request != null ? JObject.Parse(Request.Replace("\\", "")) : null;

            if (request == null || request.IsNullOrEmpty())
                throw new DataMisalignedException("Cannot find Request");//TODO: Why DataMisalignedException? What is this?

            var functionName = (string)request["function_name"];
            var functionHttpMethod = (string)request["function_http_method"];
            var functionBody = (string)request["function_body"];
            var functionUrl = await _functionHelper.GetFunctionUrl(functionName);

            if (string.IsNullOrWhiteSpace(functionUrl))
                throw new Exception("FunctionUrl not found");

            var response = await _functionHelper.Run(functionUrl, functionHttpMethod, functionBody);
            var result = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception("Function error occurred. StatusCode: " + response.StatusCode + " Content: " + result);

            Response = result;

            return ExecutionResult.Next();
        }
    }
}
