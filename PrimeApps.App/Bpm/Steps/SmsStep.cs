using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Steps
{
    public class SmsStep : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {
            var data = new JObject();
            data["id"] = 1;
            data["value"] = "Galip";

            return ExecutionResult.Outcome(data);
        }
    }
}
