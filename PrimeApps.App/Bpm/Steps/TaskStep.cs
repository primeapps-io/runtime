using System;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.App.Bpm.Steps
{
    public class TaskStep : StepBodyAsync
    {
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private IRecordHelper _recordHelper;

        public JObject Record { get; set; }

        public TaskStep(IRecordRepository recordRepository, IModuleRepository moduleRepository, IRecordHelper recordHelper)
        {
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _recordHelper = recordHelper;
        }

        public override async Task<ExecutionResult> RunAsync(IStepExecutionContext context)
        {
            var moduleEntity = await _moduleRepository.GetByNameBasic("activities");
            var record = new JObject();
            record["owner"] = (int)Record["record"]["owner"];
            record["subject"] = "The subject is " + (string)Record["record"]["name"];

            var resultCreate = await _recordRepository.Create(record, moduleEntity);

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            return ExecutionResult.Next();
        }
    }
}
