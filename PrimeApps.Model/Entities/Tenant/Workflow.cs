using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("workflows")]
    public class Workflow : BaseEntity
    {
        [Column("module_id"), ForeignKey("Module")]
        public int ModuleId { get; set; }

        [Column("name"), MaxLength(200), Required]
        public string Name { get; set; }

        [Column("frequency"), Required]
        public WorkflowFrequency Frequency { get; set; }

        [Column("process_filter"), Required]
        public WorkflowProcessFilter ProcessFilter { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("changed_field"), MaxLength(200)]
        public string ChangedField { get; set; }

        [Column("operations"), MaxLength(50), Required]
        public string Operations { get; set; }
         
        [Column("environment"), MaxLength(10)]
        public string Environment { get; set; }

        public virtual Module Module { get; set; }

        public virtual ICollection<WorkflowFilter> Filters { get; set; }

        public WorkflowNotification SendNotification { get; set; }

        public WorkflowTask CreateTask { get; set; }

        public WorkflowUpdate FieldUpdate { get; set; }

        public WorkflowWebhook WebHook { get; set; }

        public virtual ICollection<WorkflowLog> Logs { get; set; }

        [NotMapped]
        public string[] OperationsArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Operations))
                    return null;

                return Operations.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    Operations = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }
    }
}
