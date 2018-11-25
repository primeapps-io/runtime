using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Console
{
    [Table("templets")]
    public class Templet : BaseEntity
    {
        [Column("name"), MaxLength(50)]
        public string Name { get; set; }

        [Column("label"), MaxLength(400)]
        public string Label { get; set; }

        [Column("description"), MaxLength(4000)]
        public string Description { get; set; }

        [Column("logo")]
        public string Logo { get; set; }
    }
}
