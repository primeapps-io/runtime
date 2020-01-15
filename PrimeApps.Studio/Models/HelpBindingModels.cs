using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class HelpBindingModel
    {
        [Required]
        public string Template { get; set; }

        public int? ModuleId { get; set; }

        public string RouteUrl { get; set; }

        [Required]
        public string Name { get; set; }

        public ModalType ModalType { get; set; }

        public ShowType ShowType { get; set; }

        public ModuleType ModuleType { get; set; }

        public bool FirstScreen { get; set; }

        public bool CustomHelp { get; set; }

        public string HelpRelation { get; set; }

        public LanguageType Language { get; set; }
    }
}