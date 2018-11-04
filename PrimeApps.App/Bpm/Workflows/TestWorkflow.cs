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
            //Conditional sample
            //var checkValue1 = new JObject();
            //checkValue1["result"] = 1;

            //var checkValue2 = new JObject();
            //checkValue2["result"] = 2;
            
            //builder
            //    .StartWith<StartStep>()
            //    .Then<DataReadStep>()
            //    .When(data => checkValue1).Do(then => then
            //        .StartWith<DataCreateStep>()
            //    )
            //    .When(data => checkValue2).Do(then => then
            //        .StartWith<DataDeleteStep>()
            //    )
            //    .Then<SmsStep>();

            //Function sample
            //builder
            //    .StartWith<StartStep>()
            //    .Then<FunctionStep>()
            //    .Input(step => step.Request, data => "{  \"function\": {    \"name\": \"core1\",    \"methodType\": \"post\",    \"postBody\": \"fatih test\"  }}");
        }
    }
}