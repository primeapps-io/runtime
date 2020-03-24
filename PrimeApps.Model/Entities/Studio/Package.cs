using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Studio
{
    [Table("packages")]
    public class Package : BaseEntity
    {
        [Column("app_id"), Required, ForeignKey("AppDraft"), JsonProperty("app_id")]
        public int AppId { get; set; }

        [Column("status"), Required]
        public ReleaseStatus Status { get; set; }

        [Column("version"), Required]
        public string Version { get; set; }
        
        [Column("revision"), Required]
        public int Revision { get; set; }

        [Column("start_time"), Required, JsonProperty("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time"), JsonProperty("end_time")]
        public DateTime EndTime { get; set; }

        [Column("settings", TypeName = "jsonb")]
        public string Settings { get; set; }

        public virtual AppDraft AppDraft { get; set; }
    }
}