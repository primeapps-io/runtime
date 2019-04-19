using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Studio
{
    // public class CommandHistory
    // {
    //     [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
    //     public int Id { get; set; }

    //     [Column("command_text")]
    //     public string CommandText { get; set; }

    //     [Column("table_name")]
    //     public string TableName { get; set; }

    //     [Column("record_id")]
    //     public int RecordId { get; set; }

    //     [Column("executed_at")]
    //     public DateTime? ExecutedAt { get; set; }

    //     [Column("created_by"), Required, ForeignKey("CreatedBy")]
    //     public int CreatedById { get; set; }
    //     public virtual StudioUser CreatedBy { get; set; }
    // }
}