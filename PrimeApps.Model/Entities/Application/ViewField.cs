using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Application
{
    [Table("view_fields")]
    public class ViewField : BaseEntity
    {
        [Column("view_id"), ForeignKey("View")]
        public int ViewId { get; set; }

        [Column("field"), MaxLength(120), Required]
        public string Field { get; set; }

        [Column("order"), Required]
        public int Order { get; set; }

        public virtual View View { get; set; }
    }
}
