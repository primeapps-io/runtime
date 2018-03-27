using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Models
{
    public class SettingBindingModel
    {
        [Required]
        public SettingType Type { get; set; }
        
        public int? UserId { get; set; }

        [Required]
        public string Key { get; set; }

        [Required]
        public string Value { get; set; }
    }
}