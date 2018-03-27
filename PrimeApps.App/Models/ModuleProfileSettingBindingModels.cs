using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Models
{
    public class ModuleProfileSettingBindingModels
    {
        [Required, Range(1, int.MaxValue)]
        public int ModuleId { get; set; }

        public string Profiles { get; set; }

        [Required, StringLength(50)]
        public string LabelEnSingular { get; set; }

        [Required, StringLength(50)]
        public string LabelTrSingular { get; set; }

        [Required, StringLength(50)]
        public string LabelEnPlural { get; set; }

        [Required, StringLength(50)]
        public string LabelTrPlural { get; set; }

        [Required]
        public short Order { get; set; }

        [StringLength(100)]
        public string MenuIcon { get; set; }

        public bool Display { get; set; }
    }
}