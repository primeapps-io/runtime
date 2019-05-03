using Newtonsoft.Json.Linq;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Steps
{
    public class StartStep : StepBody
    {
        public string Request { get; set; }
        public string Response { get; set; }

        public override ExecutionResult Run(IStepExecutionContext context)
        { 
            return ExecutionResult.Next();
        }

    }
}
