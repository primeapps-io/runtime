using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Entities.Application
{
    [Table("workflow_tasks")]
    public class WorkflowTask
    {
        [JsonIgnore]
        [Column("workflow_id"), Key]
        public int WorkflowId { get; set; }

        [Column("owner"), Required]
        public int Owner { get; set; }

        [Column("subject"), MaxLength(2000), Required]
        public string Subject { get; set; }

        [Column("task_due_date"), Required]
        public int TaskDueDate { get; set; }

        [Column("task_status")]
        public int? TaskStatus { get; set; }

        [Column("task_priority")]
        public int? TaskPriority { get; set; }

        [Column("task_notification")]
        public int? TaskNotification { get; set; }

        [Column("task_reminder")]
        public DateTime? TaskReminder { get; set; }

        [Column("reminder_recurrence")]
        public int? ReminderRecurrence { get; set; }

        [Column("description"), MaxLength(2000)]
        public string Description { get; set; }

        public virtual Workflow Workflow { get; set; }

        [NotMapped]
        public UserBasic OwnerUser { get; set; }
    }
}
