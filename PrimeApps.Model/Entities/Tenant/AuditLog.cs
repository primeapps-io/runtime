using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Application
{
    [Table("audit_logs")]
    public class AuditLog : BaseEntity
    {
        [Column("audit_type")]
        public AuditType AuditType { get; set; }

        [JsonIgnore]
        [Column("module_id"), ForeignKey("Module")]//, Index]
        public int? ModuleId { get; set; }

        [Column("record_id")]
        public int? RecordId { get; set; }

        [Column("record_name"), MaxLength(50)]
        public string RecordName { get; set; }
        
        [Column("record_action_type")]
        public RecordActionType RecordActionType { get; set; }

        [Column("setup_action_type")]
        public SetupActionType SetupActionType { get; set; }

        public virtual Module Module { get; set; }
    }
}
