using Newtonsoft.Json.Linq;
using PrimeApps.App.Bpm.Steps;
using System;
using PrimeApps.App.Models;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Workflows
{
    public class TestWorkflow : IWorkflow<BpmReadDataModel>
    {
        public string Id => "TestWorkflow";

        public int Version => 1;

        public void Build(IWorkflowBuilder<BpmReadDataModel> builder)
        {
            //Conditional sample
            builder
                .StartWith<StartStep>()
                .Then<DataReadStep>()
                .Input(step => step.Request, data => "{\"data_read\": {\"record_key\": \"title\"}}")
                .Output(data => data.ConditionValue, step => step.Response)
                .If(data => data.ConditionValue == "Bay").Do(then => then
                    .StartWith<DataCreateStep>()
                )
                .If(data => data.ConditionValue == "Bayan").Do(then => then
                    .StartWith<DataDeleteStep>()
                )
                .Then<SmsStep>()
                .Then(x => ExecutionResult.Next())
                .EndWorkflow();

            //Function sample
            //builder
            //    .StartWith<StartStep>()
            //    .Then<FunctionStep>()
            //    .Input(step => step.Request, data => "{  \"function\": {    \"name\": \"core1\",    \"methodType\": \"post\",    \"postBody\": \"fatih test\"  }}");
        }
    }
}