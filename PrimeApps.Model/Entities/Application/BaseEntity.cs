using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Application
{
    public class BaseEntity
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }

        [Column("created_by"), ForeignKey("CreatedBy")]//, Index]
        public int CreatedById { get; set; }

        [Column("updated_by"), ForeignKey("UpdatedBy")]//, Index]
        public int? UpdatedById { get; set; }

        [Column("created_at"), Required]//, Index]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]//, Index]
        public DateTime? UpdatedAt { get; set; }

        [Column("deleted")]//, Index]
        public bool Deleted { get; set; }

        public virtual TenantUser CreatedBy { get; set; }

        public virtual TenantUser UpdatedBy { get; set; }
    }
}
