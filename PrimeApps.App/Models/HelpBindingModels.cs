using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrimeApps.App.Models
{
    public class HelpBindingModel
    {
        [Required, StringLength(4000)]
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

    }
}