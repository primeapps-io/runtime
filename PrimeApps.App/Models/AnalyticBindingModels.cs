using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Models
{
    public class AnalyticBindingModel
    {
        [Required, StringLength(50)]
        public string Label { get; set; }

        [Required]
        public string PbixUrl { get; set; }

        public AnalyticSharingType SharingType { get; set; }

        public List<int> Shares { get; set; }

        public string MenuIcon { get; set; }
    }
}