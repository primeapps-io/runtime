using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.Console.Models
{
    public class FunctionBindingModel
    {
        [Required, StringLength(200)]
        public string Name { get; set; }

        public string Dependencies { get; set; }

        public string Function { get; set; }

        [Required]
        public string Handler { get; set; }

        [Required]
        public FunctionRuntime Runtime { get; set; }

        [Required]
        public FunctionContentType ContentType { get; set; }
    }
}