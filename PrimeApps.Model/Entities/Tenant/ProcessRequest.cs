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
        [Column("process_id"), ForeignKey("Process")]
        public int ProcessId { get; set; }

        [Column("module")]
        public string Module { get; set; }

        [Column("record_id")]
        public int RecordId { get; set; }

        [Column("process_status")]
        public ProcessStatus Status { get; set; }

        [Column("operation_type")]
        public OperationType OperationType { get; set; }

        [Column("process_status_order")]
        public int ProcessStatusOrder { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        public virtual Process Process { get; set; }
        
    }
}
