using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories;
using System;
using System.Threading.Tasks;
using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Steps
{
    public class TaskStep : StepBodyAsync
    {
        private IServiceScopeFactory _serviceScopeFactory;
        private IConfiguration _configuration;

        public JObject Record { get; set; }

        public TaskStep(IServiceScopeFactory serviceScopeFactory, IConfiguration configuration)
        {
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            if (context == null)
                throw new NullReferenceException();

            if (context.Workflow.Reference == null)
                throw new NullReferenceException();

            var appUser = JsonConvert.DeserializeObject<JObject>(context.Workflow.Reference);

            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<TenantDBContext>();
                using (var userRepository = new UserRepository(databaseContext, _configuration))
                {
                    userRepository.CurrentUser = new Model.Helpers.CurrentUser()
                    {
                        TenantId = (int)appUser.GetValue("tenant_id"),
                        UserId = (int)appUser.GetValue("user_id")
                    };
                        //Example
                        //var result = await userRepository.GetById(2);
                }
            }



            //var moduleEntity =  _moduleRepository.GetByNameBasic("activities");
            var record = new JObject();
            //record["owner"] = (int)Record["id"];
            record["subject"] = "The subject is Real!"; //+ (string)Record["record"]["name"];

            //////var resultCreate =  _recordRepository.Create(record, moduleEntity);

            //////if (resultCreate < 1)
            //////    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            return ExecutionResult.Next();
        }
    }
}
