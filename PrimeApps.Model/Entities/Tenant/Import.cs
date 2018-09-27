using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("imports")]
    public class Import : BaseEntity
    {
        [JsonIgnore]
        [Column("module_id"), ForeignKey("Module")]//, Index]
        public int ModuleId { get; set; }

        [Column("total_count"), Required]
        public int TotalCount { get; set; }

        [Column("excel_url")]
        public string ExcelUrl { get; set; }

        public virtual Module Module { get; set; }
    }
}
