using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("calculations")]
    public class Calculation : BaseEntity
    {
        [JsonIgnore]
        [Column("module_id"), ForeignKey("Module")]//, Index]
        public int ModuleId { get; set; }

        [Column("result_field"), MaxLength(50), Required]
        public string ResultField { get; set; }

        [Column("field1"), MaxLength(50)]
        public string Field1 { get; set; }

        [Column("field2"), MaxLength(50)]
        public string Field2 { get; set; }

        [Column("custom_value")]
        public double? CustomValue { get; set; }

        [Column("operator"), MaxLength(1), Required]
        public string Operator { get; set; }

        [Column("order"), Required]
        public short Order { get; set; }

        public virtual Module Module { get; set; }
    }
}
