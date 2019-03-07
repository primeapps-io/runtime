using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Studio
{
    public class BaseEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }

        [Column("created_by"), Required, ForeignKey("CreatedBy")]
        public int CreatedById { get; set; }

        [Column("updated_by"), ForeignKey("UpdatedBy")]
        public int? UpdatedById { get; set; }

        [Column("created_at"), Required]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted")]
        public bool Deleted { get; set; }

        public virtual StudioUser CreatedBy { get; set; }

        public virtual StudioUser UpdatedBy { get; set; }
    }
}
