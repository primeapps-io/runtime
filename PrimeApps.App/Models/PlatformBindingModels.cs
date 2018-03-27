using System.ComponentModel.DataAnnotations;

namespace PrimeApps.App.Models
{
    public class AppBindingModel
    {
        [Required, StringLength(400)]
        public string Name { get; set; }

        [StringLength(4000)]
        public string Description { get; set; }

        [StringLength(4000)]
        public string Logo { get; set; }

        public int? TemplateId { get; set; }
    }
}