using PrimeApps.Model.Common.Annotations;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Models
{
    public class PlatformWorkflowBindingModels
    {
        [Required, Range(1, int.MaxValue)]
        public int AppId { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        [Required]
        public WorkflowFrequency Frequency { get; set; }

        public bool Active { get; set; }

        [RequiredCollection]
        public string[] Operations { get; set; }

        public PlatformWorkflowActionsBindingModel Actions { get; set; }
    }

    public class PlatformWorkflowActionsBindingModel
    {
        public PlatformWorkflowWebhookBindingModel WebHook { get; set; }
    }

    public class PlatformWorkflowWebhookBindingModel
    {
        [Required, StringLength(300)]
        public string CallbackUrl { get; set; }

        [Required]
        public WorkflowHttpMethod MethodType { get; set; }

        public string Parameters { get; set; }
    }
}
