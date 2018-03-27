using System;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Common.Warehouse;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("warehouses")]
    public class PlatformWarehouse
    {
        [Column("id")]
        public Guid Id { get; set; }

        [Column("tenant_id")]//]//, Index]
        public int TenantId { get; set; }

        [Column("database_name")]//]//, Index]
        public string DatabaseName { get; set; }

        [Column("database_user")]
        public string DatabaseUser { get; set; }

        [Column("powerbi_workspace_id")]
        public string PowerbiWorkspaceId { get; set; }

        [Column("completed")]//]//, Index]
        public bool Completed { get; set; }

        internal static void SetCompleted(PlatformWarehouse warehouseEntity, string userEmail)
        {
            throw new NotImplementedException();
        }

        internal static PlatformWarehouse Create(WarehouseCreateRequest request)
        {
            throw new NotImplementedException();
        }
    }
}
