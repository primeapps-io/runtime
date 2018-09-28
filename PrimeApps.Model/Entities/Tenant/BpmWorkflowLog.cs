using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("bpm_workflow_logs")]
    public class BpmWorkflowLog : BaseEntity
    {
        [Column("code"), ForeignKey("BpmWorkflow")]
        public int WorkflowId { get; set; }

        [Column("module_id"), Required]
        public int ModuleId { get; set; }

        [Column("record_id"), Required]
        public int RecordId { get; set; }

        public virtual BpmWorkflow BpmWorkflow { get; set; }
    }
}
