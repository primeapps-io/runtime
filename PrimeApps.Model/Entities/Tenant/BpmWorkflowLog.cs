using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Application
{
    [Table("bpm_workflow_logs")]
    public class BpmWorkflowLog : BaseEntity
    {
        [Column("code"), ForeignKey("BpmWorkflow")]//, Index]
        public int WorkflowId { get; set; }

        [Column("module_id"), Required]//, Index]
        public int ModuleId { get; set; }

        [Column("record_id"), Required]//, Index]
        public int RecordId { get; set; }

        public virtual BpmWorkflow BpmWorkflow { get; set; }
    }
}
