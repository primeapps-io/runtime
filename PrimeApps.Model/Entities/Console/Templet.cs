using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Console
{
    [Table("templets")]
    public class Templet : BaseEntity
    {
        [Column("category_id"), ForeignKey("Category")]
        public int CategoryId { get; set; }

        [Column("label"), MaxLength(400)]
        public string Label { get; set; }

        [Column("description"), MaxLength(4000)]
        public string Description { get; set; }

        [Column("logo")]
        public string Logo { get; set; }

        [Column("image")]
        public string Image { get; set; }

        public virtual TempletCategory Category { get; set; }
    }
}
