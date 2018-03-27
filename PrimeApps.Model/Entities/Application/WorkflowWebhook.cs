using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Application
{
    [Table("workflow_webhooks")]
    public class WorkflowWebhook
    {
        [JsonIgnore]
        [Column("workflow_id"), Key]
        public int WorkflowId { get; set; }

        [Column("callback_url"), MaxLength(500), Required]
        public string CallbackUrl { get; set; }

        [Column("method_type"), Required, DefaultValue(WorkflowHttpMethod.Post)]
        public WorkflowHttpMethod MethodType { get; set; }

        [Column("parameters")]
        public string Parameters { get; set; }

        public virtual Workflow Workflow { get; set; }
    }
}