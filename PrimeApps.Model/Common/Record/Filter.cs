using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Record
{
    public class Filter
    {
        [Required, StringLength(120)]
        [RegularExpression(@"[A-Za-z0-9._]+", ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field { get; set; }

        [Required]
        public Operator Operator { get; set; }

        [Required]
        public object Value { get; set; }

        [Required]
        public int No { get; set; }

        public bool DocumentSearch { get; set; }
    }
}
