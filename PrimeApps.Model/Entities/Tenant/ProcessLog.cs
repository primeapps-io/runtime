using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("process_logs")]
    public class ProcessLog : BaseEntity
    {
        [Column("process_id"), ForeignKey("Process")]
        public int ProcessId { get; set; }

        [Column("module_id"), Required]
        public int ModuleId { get; set; }

        [Column("record_id"), Required]
        public int RecordId { get; set; }

        public virtual Process Process { get; set; }
    }
}
