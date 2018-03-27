using System.ComponentModel.DataAnnotations;

namespace PrimeApps.App.Models
{
    public class OutlookBindingModel
    {
        [Required]
        public string Module { get; set; }

        [Required]
        public string EmailField { get; set; }
    }
}