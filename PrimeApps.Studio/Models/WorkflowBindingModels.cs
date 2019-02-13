using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Common.Annotations;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class WorkflowBindingModel
    {
        [Required, Range(1, int.MaxValue)]
        public int ModuleId { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        [Required]
        public WorkflowFrequency Frequency { get; set; }

        [Required]
        public WorkflowProcessFilter ProcessFilter { get; set; }

        public bool Active { get; set; }

        [StringLength(200)]
        public string ChangedField { get; set; }

        public bool DeleteLogs { get; set; }

        [RequiredCollection]
        public string[] Operations { get; set; }

        public List<Filter> Filters { get; set; }

        public ActionsBindingModel Actions { get; set; }
    }

    public class ActionsBindingModel
    {
        public WorkflowNotificationBindingModel SendNotification { get; set; }

        public WorkflowTaskBindingModel CreateTask { get; set; }

        public WorkflowUpdateBindingModel FieldUpdate { get; set; }

        public WorkflowWebhookBindingModel WebHook { get; set; }
    }

    public class WorkflowNotificationBindingModel
    {
        [Required, StringLength(200)]
        public string Subject { get; set; }

        [Required, StringLength(32000)]
        public string Message { get; set; }

        [RequiredCollection]
        public string[] Recipients { get; set; }

        public string[] CC { get; set; }

        public string[] Bcc { get; set; }

        public int? Schedule { get; set; }
    }

    public class WorkflowTaskBindingModel
    {
        [Required]
        public int Owner { get; set; }

        [Required, StringLength(2000)]
        public string Subject { get; set; }

        [Required]
        public int TaskDueDate { get; set; }

        public int? TaskStatus { get; set; }

        public int? TaskPriority { get; set; }

        public int? TaskNotification { get; set; }

        public DateTime? TaskReminder { get; set; }

        public int? ReminderRecurrence { get; set; }

        [StringLength(2000)]
        public string Description { get; set; }
    }

    public class WorkflowUpdateBindingModel
    {
        [Required, StringLength(120)]
        public string Module { get; set; }

        [Required, StringLength(120)]
        public string Field { get; set; }

        [Required, StringLength(2000)]
        public string Value { get; set; }
    }

    public class WorkflowWebhookBindingModel
    {
        [Required, StringLength(300)]
        public string CallbackUrl { get; set; }

        [Required]
        public WorkflowHttpMethod MethodType { get; set; }

        public string Parameters { get; set; }
    }
}