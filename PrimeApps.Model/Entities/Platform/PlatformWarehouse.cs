using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("warehouses")]
    public class PlatformWarehouse : BaseEntity
    {
        [Column("tenant_id"), ForeignKey("Tenant")]
        public int TenantId { get; set; }

        [Column("database_name")]
        public string DatabaseName { get; set; }

        [Column("database_user")]
        public string DatabaseUser { get; set; }

        [Column("powerbi_workspace_id")]
        public string PowerbiWorkspaceId { get; set; }

        [Column("completed")]
        public bool Completed { get; set; }

        public virtual Tenant Tenant { get; set; }
    }
}
