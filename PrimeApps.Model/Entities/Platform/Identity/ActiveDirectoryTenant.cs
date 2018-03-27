using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform.Identity
{
    [Table("active_directory_tenants")]
    public class ActiveDirectoryTenant
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("name"), Required, MaxLength(500)]
        public string Name { get; set; }

        [Column("issuer"), Required, MaxLength(500)]//]//, Index]
        public string Issuer { get; set; }

        [Column("admin_consented"), Required]
        public bool AdminConsented { get; set; }

        [Column("created_at"), Required]//]//, Index]
        public DateTime CreatedAt { get; set; }

        [Column("tenant_id"), Required]//]//, Index]
        public int TenantId { get; set; }

        [Column("confirmed")]//]//, Index]
        public bool Confirmed { get; set; }
    }
}
