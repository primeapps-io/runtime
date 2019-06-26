using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Studio
{
    [Table("releases")]
    public class Release : BaseEntity
    {
        [Column("app_id"), Required, ForeignKey("AppDraft")]
        public int AppId { get; set; }

        [Column("status"), Required]
        public ReleaseStatus Status { get; set; }

        [Column("version"), Required]
        public string Version { get; set; }

        [Column("start_time"), Required]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime EndTime { get; set; }

        [Column("settings", TypeName = "jsonb")]
        public string Settings { get; set; }

        [Column("published")]
        public bool Published { get; set; }

        public virtual AppDraft AppDraft { get; set; }
    }
}