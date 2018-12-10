using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Console
{
    [Table("templet_categories")]
    public class TempletCategory : BaseEntity
    {
        [Column("label"), MaxLength(400)]
        public string Label { get; set; }

        [Column("description"), MaxLength(4000)]
        public string Description { get; set; }

        [Column("image")]
        public string Image { get; set; }
    }
}
