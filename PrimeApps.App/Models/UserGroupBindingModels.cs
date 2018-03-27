using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Common.Annotations;

namespace PrimeApps.App.Models
{
    public class UserGroupBindingModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(500)]
        public string Description { get; set; }

        [RequiredCollection]
        public List<int> UserIds { get; set; }
    }
}