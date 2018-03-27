using NpgsqlTypes;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Application
{
    [Table("notifications")]
    public class Notification : BaseEntity
    {

        [Column("type"), Required]
        [Index]
        public NotificationType NotificationType { get; set; }

        [Column("module_id"), ForeignKey("Module")]
        public int ModuleId { get; set; }

        [Column("rev")]
        public string Rev { get; set; }

        [Column("ids"), Required]
        public string Ids { get; set; }

        [Column("query")]
        public string Query { get; set; }

        [Column("status"), Required]
        public NotificationStatus Status { get; set; }

        [Column("template"), Required]
        public string Template { get; set; }

        [Column("lang"), Required]
        public string Lang { get; set; }

        [Column("queue_date"), Required]
        public DateTime QueueDate { get; set; }

        [Column("phone_field"), MaxLength(50)]
        public string PhoneField { get; set; }

        [Column("email_field"), MaxLength(50)]
        public string EmailField { get; set; }

        [Column("sender_alias"), MaxLength(50)]
        public string SenderAlias { get; set; }

        [Column("sender_email"), MaxLength(50)]
        public string SenderEmail { get; set; }

        [Column("attachment_container"), MaxLength(50)]
        public string AttachmentContainer { get; set; }

        [Column("subject"), MaxLength(128)]
        public string Subject { get; set; }

        [Column("attachment_link"), MaxLength(500)]
        public string AttachmentLink { get; set; }

        [Column("attachment_name"), MaxLength(50)]
        public string AttachmentName { get; set; }

        [Column("result")]
        public string Result { get; set; }

        public virtual Module Module { get; set; }
    }
}
