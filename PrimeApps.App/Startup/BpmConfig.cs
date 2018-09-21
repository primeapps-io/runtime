using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Bpm.Workflows;
using System.Collections.Generic;
using WorkflowCore.Interface;

namespace PrimeApps.App
{
    public partial class Startup
    {
        public void BpmConfiguration(IApplicationBuilder app, IConfiguration configuration)
        {
            var host = app.ApplicationServices.GetService<IWorkflowHost>();

            //Register worflows here if platform needs. Use host.RegisterWorkflow method of Worflow-Core.
            //App's and tenant's worflows should be register on runtime.

            //Register TestWorkflow
           host.RegisterWorkflow<TestWorkflow, JObject>();

            host.Start();

            var data = new JObject();
            data["id"] = 1;
            data["value"] = "Merhaba";

            host.StartWorkflow("TestWorkflow", 1, data);
        }
    }
}
