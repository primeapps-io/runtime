using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Annotations;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrimeApps.App.Models
{
    public class BpmWorkflowBindingModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        public string Description { get; set; }

        public int? CategoryId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        [Required]
        public BpmTriggerType TriggerType { get; set; }

        [Required]
        public string RecordOperations { get; set; }

        [Required]
        public WorkflowFrequency Frequency { get; set; }

        [MaxLength(4000)]
        public string[] ChangedFields { get; set; }

        public bool CanStartManuel { get; set; }
        
        public JObject DefinitionJson { get; set; }

        public string DiagramJson { get; set; }



    }
}