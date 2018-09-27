using Newtonsoft.Json.Linq;
using PrimeApps.App.Bpm.Steps;
using System;
using WorkflowCore.Interface;

namespace PrimeApps.App.Bpm.Workflows
{
    public class TestWorkflow : IWorkflow<JObject>
    {
        public string Id => "TestWorkflow";

        public int Version => 1;

        public void Build(IWorkflowBuilder<JObject> builder)
        {
            builder
                .StartWith<DataReadStep>();
                //.Input(step => step.Request, data => data)
                // //.Output(data => data, step => step)
                //.Then<TaskStep>()
                //.Input(step => step.Record, data => data);
        }
    }
}