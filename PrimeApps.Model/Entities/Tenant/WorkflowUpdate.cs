using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("workflow_updates")]
    public class WorkflowUpdate
    {
        [JsonIgnore]
        [Column("workflow_id"), Key]
        public int WorkflowId { get; set; }

        [Column("module"), MaxLength(120), Required]
        public string Module { get; set; }

        [Column("field"), MaxLength(120), Required]
        public string Field { get; set; }

        [Column("value"), MaxLength(2000), Required]
        public string Value { get; set; }

        public virtual Workflow Workflow { get; set; }
    }
}
