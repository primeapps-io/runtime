using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Application
{
    [Table("process_approvers")]
    public class ProcessApprover : BaseEntity
    {
        [JsonIgnore]
        [Column("process_id"), ForeignKey("Process")]//, Index]
        public int ProcessId { get; set; }

        [JsonIgnore]
        [Column("user_id"), ForeignKey("User")]
        public int UserId { get; set; }

        [Column("order")]
        public short Order { get; set; }

        public virtual Process Process { get; set; }

        public virtual TenantUser User { get; set; }
    }
}
