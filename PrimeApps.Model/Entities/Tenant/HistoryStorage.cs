using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("history_storage")]
    public class HistoryStorage
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }

        [Column("mime_type")]
        public string MimeType { get; set; }

        [Column("operation")]
        public string Operation { get; set; }

        [Column("file_name")]
        public string FileName { get; set; }

        [Column("unique_name")]
        public string UniqueName { get; set; }

        [Column("path")]
        public string Path { get; set; }

        [Column("executed_at")]
        public DateTime? ExecutedAt { get; set; }

        [Column("created_by"), Required]
        public string CreatedByEmail { get; set; }
        
        [Column("deleted")]
        public bool Deleted { get; set; }
    }
}