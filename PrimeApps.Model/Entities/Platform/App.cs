using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("apps")]
    public class App
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("name"), MaxLength(400)]
        public string Name { get; set; }

        [Column("description"), MaxLength(4000)]
        public string Description { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("logo")]
        public string Logo { get; set; }

        [Column("template_id")]
        public int? TemplateId { get; set; }

        [Column("deleted")]
        public bool Deleted { get; set; }
    }
}
