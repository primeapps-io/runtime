using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("field_filters")]
    public class FieldFilter : BaseEntity
    {
        [Column("field_id"), ForeignKey("Field")]
        public int FieldId { get; set; }

        [Column("filter_field"), MaxLength(120), Required]
        public string FilterField { get; set; }

        public virtual Field Field { get; set; }

        [Required]
        public Operator Operator { get; set; }

        [Required, MaxLength(100)]
        public string Value { get; set; }

        [Required]
        public int No { get; set; }

    }
}
