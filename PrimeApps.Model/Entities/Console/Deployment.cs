using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Console
{
    [Table("deployments")]
    public class Deployment : BaseEntity
    {
        [Column("app_id"), Required, ForeignKey("AppDraft")]
        public int AppId { get; set; }

        [Column("status"), Required]
        public DeploymentStatus Status { get; set; }

        [Column("version"), Required]
        public string Version { get; set; }

        [Column("start_time"), Required]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime EndTime { get; set; }

        public virtual AppDraft AppDraft { get; set; }
    }
}
