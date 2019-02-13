using System;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class DeploymentFunctionBindingModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public int FunctionId { get; set; }

        [Required]
        public DeploymentStatus Status { get; set; }

        [Required]
        public string Version { get; set; }

        [Required]
        public DateTime StartTime { get; set; }
        
        public DateTime EndTime { get; set; }
    }
}