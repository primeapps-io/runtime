using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("deployments_function")]

    public class DeploymentFunction : BaseEntity
    {
        [Column("function_id"), Required, ForeignKey("Function")]
        public int FunctionId { get; set; }

        [Column("status"), Required]
        public DeploymentStatus Status { get; set; }

        [Column("revision"), Required]
        public int Revision { get; set; }

        [Column("build_number"), Required]
        public int BuildNumber { get; set; }

        [Column("version"), Required]
        public string Version { get; set; }

        [Column("start_time"), Required]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime EndTime { get; set; }
        
        public virtual Function Function { get; set; }
    }
}
