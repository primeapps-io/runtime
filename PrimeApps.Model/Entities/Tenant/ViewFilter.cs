using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("view_filters")]
    public class ViewFilter : BaseEntity
    {
        [Column("view_id"), ForeignKey("View")]
        public int ViewId { get; set; }

        [Column("field"), MaxLength(120), Required]
        public string Field { get; set; }

        [Required]
        public Operator Operator { get; set; }

        [Required, MaxLength(100)]
        public string Value { get; set; }

        [Required]
        public int No { get; set; }

        public virtual View View { get; set; }
    }
}
