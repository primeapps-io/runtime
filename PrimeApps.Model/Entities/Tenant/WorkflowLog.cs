using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("workflow_logs")]
    public class WorkflowLog : BaseEntity
    {
        [Column("workflow_id"), ForeignKey("Workflow")]
        public int WorkflowId { get; set; }

        [Column("module_id"), Required]
        public int ModuleId { get; set; }

        [Column("record_id"), Required]
        public int RecordId { get; set; }

        public virtual Workflow Workflow { get; set; }
    }
}
