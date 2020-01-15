using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("history_database")]
    public class HistoryDatabase
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }

        [Column("command_text")]
        public string CommandText { get; set; }

        [Column("table_name")]
        public string TableName { get; set; }

        [Column("tag")]
        public string Tag { get; set; }

        [Column("command_id")]
        public Guid CommandId { get; set; }

        [Column("executed_at")]
        public DateTime? ExecutedAt { get; set; }

        [Column("created_by"), Required]
        public string CreatedByEmail { get; set; }

        [Column("deleted")]
        public bool Deleted { get; set; }
    }
}