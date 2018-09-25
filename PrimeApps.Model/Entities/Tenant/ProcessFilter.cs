using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("process_filters")]
    public class ProcessFilter : BaseEntity
    {
        [JsonIgnore]
        [Column("process_id"), ForeignKey("Process")]//, Index]
        public int ProcessId { get; set; }

        [Column("field"), MaxLength(120), Required]
        public string Field { get; set; }

        [Required]
        public Operator Operator { get; set; }

        [Required, MaxLength(100)]
        public string Value { get; set; }

        [Required]
        public int No { get; set; }

        public virtual Process Process { get; set; }
    }
}
