using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Steps
{
    public class DataReadStep : StepBody
    {
        public JObject Request { get; set; }

        public JObject Response { get; set; }
        
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            Response = new JObject();
            Response = new JObject();
            Response["owner"] = Request["owner"];
            Response["name"] = Request["name"];

            return ExecutionResult.Outcome(Response);
        }
    }
}
