using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Application
{
    [Table("field_validations")]
    public class FieldValidation
    {
        [JsonIgnore]
        [Column("field_id"), Key]
        public int FieldId { get; set; }

        [Column("required")]
        public bool? Required { get; set; }

        [Column("readonly")]
        public bool? Readonly { get; set; }

        [Column("min_length")]
        public short? MinLength { get; set; }

        [Column("max_length")]
        public short? MaxLength { get; set; }

        [Column("min")]
        public double? Min { get; set; }

        [Column("max")]
        public double? Max { get; set; }

        [Column("pattern"), MaxLength(200)]
        public string Pattern { get; set; }

        [Column("unique")]
        public bool? Unique { get; set; }

        public virtual Field Field { get; set; }
    }
}
