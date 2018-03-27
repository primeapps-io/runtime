using System.ComponentModel.DataAnnotations;

namespace PrimeApps.Model.Common.Warehouse
{
    public class WarehouseCreateRequest
    {
        [Required]
        public int TenantId { get; set; }

        [Required, StringLength(20)]
        public string DatabaseName { get; set; }

        [Required, StringLength(20)]
        public string DatabaseUser { get; set; }

        [Required, StringLength(20)]
        public string DatabasePassword { get; set; }

        public string PowerBiWorkspaceId { get; set; }
    }
}
