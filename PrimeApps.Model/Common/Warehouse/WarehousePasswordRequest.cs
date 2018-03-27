using System.ComponentModel.DataAnnotations;

namespace PrimeApps.Model.Common.Warehouse
{
    public class WarehousePasswordRequest
    {
        [Required, StringLength(20)]
        public string DatabasePassword { get; set; }
    }
}
