﻿using WorkflowCore.Interface;
using WorkflowCore.Models;

namespace PrimeApps.App.Bpm.Steps
{
    public class StartStep : StepBody
    {
        public override ExecutionResult Run(IStepExecutionContext context)
        {



            return ExecutionResult.Next();
        }
        
    }
}
