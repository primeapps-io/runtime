using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("process_requests")]
    public class ProcessRequest : BaseEntity
    {

        [JsonIgnore]
        [Column("process_id"), ForeignKey("Process")]//, Index]
        public int ProcessId { get; set; }

        [Column("module")]//, Index]
        public string Module { get; set; }

        [Column("record_id")]//, Index]
        public int RecordId { get; set; }

        [Column("process_status")]//, Index]
        public ProcessStatus Status { get; set; }

        [Column("operation_type")]//, Index]
        public OperationType OperationType { get; set; }

        [Column("process_status_order")]//, Index]
        public int ProcessStatusOrder { get; set; }

        [Column("active")]//, Index]
        public bool Active { get; set; }

        public virtual Process Process { get; set; }
        
    }
}
