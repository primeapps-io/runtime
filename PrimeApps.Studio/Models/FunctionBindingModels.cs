using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class FunctionBindingModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, StringLength(300)]
        public string Label { get; set; }

        public string Dependencies { get; set; }

        public string Function { get; set; }

        [Required]
        public string Handler { get; set; }

        [Required]
        public FunctionRuntime Runtime { get; set; }

        [Required]
        public FunctionContentType ContentType { get; set; }

        [Required]
        public PublishStatus Status { get; set; }
    }
}