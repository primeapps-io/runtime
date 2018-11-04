using Newtonsoft.Json.Linq;
using System;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Steps
{
    public class DataReadStep : StepBody
    {
        public string Request { get; set; }

        public string Response { get; set; }

        public DateTime Date { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            //Response = new JObject();
            //Response = new JObject();
            //Response["owner"] = Request["id"];
            //Response["name"] = Request["value"];
            //   ExecutionResult.Outcome(Response).OutcomeValue = Response;

            var checkValue1 = new JObject();
            checkValue1["result"] = 2;

            return ExecutionResult.Outcome(checkValue1);
        }
    }
}
