using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrimeApps.App.Models
{
    public class BpmWorkflowBindingModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string DefinitionJson { get; set; }
    }
}