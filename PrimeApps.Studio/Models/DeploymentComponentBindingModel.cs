using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Studio.Models
{
    public class DeploymentComponentBindingModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int ComponentId { get; set; }

        [Required]
        public ReleaseStatus Status { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }
    }
}
