using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Common.Annotations;

namespace PrimeApps.App.Models
{
    public class PicklistBindingModel
    {
        [Required, StringLength(50)]
        public string LabelEn { get; set; }

        [Required, StringLength(50)]
        public string LabelTr { get; set; }

        [RequiredCollection]
        public List<PicklistItemBindingModel> Items { get; set; }
    }

    public class PicklistItemBindingModel
    {
        public int? Id { get; set; }

        [Required, StringLength(100)]
        public string LabelEn { get; set; }

        [Required, StringLength(100)]
        public string LabelTr { get; set; }

        [MaxLength(100)]
        public string Value { get; set; }

        [MaxLength(100)]
        public string Value2 { get; set; }

        [MaxLength(100)]
        public string Value3 { get; set; }

        [Required]
        public short Order { get; set; }

        public bool Inactive { get; set; }
    }
}