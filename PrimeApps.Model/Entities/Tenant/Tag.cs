using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("tags")]
    public class Tag : BaseEntity
    {
        [Column("text"), MaxLength(400), /*Index*/]
        public string Text { get; set; }

        [Column("field_id"), ForeignKey("Field"), /*Index*/]
        public int FieldId { get; set; }

        public virtual Field Field { get; set; }
    }
}
