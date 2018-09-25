using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("reminders")]

    public class Reminder : BaseEntity
    {
        [Column("reminder_scope"), Required,/* Index,*/ MaxLength(70)]
        public string ReminderScope { get; set; }

        [Column("module_id"), ForeignKey("Module")]
        public int? ModuleId { get; set; }

        [Column("record_id")]
        public int? RecordId { get; set; }

        [Column("owner")]
        public int? Owner { get; set; }

        [Column("reminder_type"), Required, MaxLength(20)]//, Index]
        public string ReminderType { get; set; }

        [Column("reminder_start"), Required]
        public DateTime ReminderStart { get; set; }

        [Column("reminder_end"), Required]
        public DateTime ReminderEnd { get; set; }

        [Column("subject"), Required, MaxLength(200)]
        public string Subject { get; set; }

        [Column("reminder_frequency")]
        public int? ReminderFrequency { get; set; }

        [Column("reminded_on")]
        public DateTime? RemindedOn { get; set; }

        [Column("rev"), MaxLength(30)]
        public string Rev { get; set; }

        public virtual Module Module { get; set; }

        [Column("timezone_offset"), DefaultValue(180)]
        public virtual int TimeZoneOffset { get; set; }
    }
}
