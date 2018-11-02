using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PrimeApps.Model.Constants
{


    public static class BpmConstants
    {
        public static string Id = "Id";
        public static string StepType = "StepType";

        public static Dictionary<string, Dictionary<string, string>> bpmStepsDefination = new Dictionary<string, Dictionary<string, string>>()
        {
            { "Timer", new Dictionary<string, string>(){{Id,"myTimerStep"}, { StepType, "WorkflowCore.Primitives.Schedule, WorkflowCore" } } },
            { "Parallel", new Dictionary<string, string>(){{ Id, "myParallel" }, { StepType, "WorkflowCore.Primitives.Sequence, WorkflowCore" } } },
            { "WaitFor", new Dictionary<string, string>(){{ Id, "myWaitFor" }, { StepType, "WorkflowCore.Primitives.WaitFor, WorkflowCore" } } }, //BPMN editorde hangi nesneye bagli olacagi belli degil?
            { "Saga", new Dictionary<string, string>(){{ Id, "mySaga" }, { StepType, "WorkflowCore.Primitives.Sequence, WorkflowCore" } } }, //BPMN editorde hangi nesneye bagli olacagi belli degil?
            { "Start", new Dictionary<string, string>(){ { Id, "StartStep" }, { StepType, "PrimeApps.App.Bpm.Steps.StartStep, PrimeApps.App" } } },
            { "Notification Task", new Dictionary<string, string>(){ { Id, "NotificationStep" },{ StepType, "PrimeApps.App.Bpm.Steps.NotificationStep, PrimeApps.App" } } },
            { "User Task", new Dictionary<string, string>(){ { Id, "TaskStep" }, { StepType, "PrimeApps.App.Bpm.Steps.TaskStep, PrimeApps.App" } } },
            { "WebHook Task", new Dictionary<string, string>(){ { Id, "WebhookStep" }, { StepType, "PrimeApps.App.Bpm.Steps.WebhookStep, PrimeApps.App" } } },
            { "Data Task", new Dictionary<string, string>(){ { Id, "DataUpdateStep" }, { StepType, "PrimeApps.App.Bpm.Steps.DataUpdateStep, PrimeApps.App" } } },
            { "Function Task", new Dictionary<string, string>(){ { Id, "FunctionStep" }, { StepType, "PrimeApps.App.Bpm.Steps.FunctionStep, PrimeApps.App" } } }
        };

        public static Dictionary<string, string> Find(string key)
        {
            var result = bpmStepsDefination.GetValueOrDefault(key);

            return result;
        }
    }
}
