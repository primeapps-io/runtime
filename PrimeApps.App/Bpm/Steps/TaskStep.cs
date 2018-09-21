using System;
using System.Linq;
using System.Threading.Tasks; 
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using WorkflowCore.Interface;
using WorkflowCore.Models;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;

namespace PrimeApps.App.Bpm.Steps
{
    public class TaskStep : StepBody
    {
        private IRecordRepository _recordRepository;
        private ICalculationHelper _calculationHelper;
        private IRecordHelper _recordHelper;

        public JObject Record { get; set; }

        public TaskStep(IRecordHelper recordHelper)
        {
            _recordHelper = recordHelper;
        }

        public override ExecutionResult Run(IStepExecutionContext context)
        {
            // _moduleRepository=(IModuleRepository)System.Reflection.Assembly.GetExecutingAssembly().GetTypes().Where(q => q.GetInterface(typeof(IServiceProvider).Name) == typeof(IModuleRepository)).FirstOrDefault();
            
            //var moduleEntity =  _moduleRepository.GetByNameBasic("activities");
            var record = new JObject();
            record["owner"] = (int)Record["record"]["owner"];
            record["subject"] = "The subject is Real!"; //+ (string)Record["record"]["name"];

            //////var resultCreate =  _recordRepository.Create(record, moduleEntity);

            //////if (resultCreate < 1)
            //////    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            return ExecutionResult.Next();
        }
    }
}
