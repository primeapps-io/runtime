using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Application
{
    [Table("field_combinations")]
    public class FieldCombination
    {
        [JsonIgnore]
        [Column("field_id"), Key]
        public int FieldId { get; set; }

        [Column("field1"), MaxLength(50), Required]
        public string Field1 { get; set; }

        [Column("field2"), MaxLength(50), Required]
        public string Field2 { get; set; }

        [Column("combination_character"), MaxLength(50)]
        public string CombinationCharacter { get; set; }

        public virtual Field Field { get; set; }
    }
}
