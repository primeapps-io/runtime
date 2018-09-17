using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Application
{
    [Table("bpm_categories")]
    public class BpmCategory : BaseEntity
    {
        [Column("name"), MaxLength(100), Required]
        public string Name { get; set; }

        [Column("order")]
        public int Order { get; set; }
    }
}