using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Annotations;
using PrimeApps.Model.Entities.Tenant;
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

        [Required, MaxLength(200)]
        public string Code { get; set; }
        
        public string Description { get; set; }

        public int? CategoryId { get; set; }

        public DateTime StartTime { get; set; }

        public DateTime EndTime { get; set; }

        public bool Active { get; set; }

        [Required]
        public BpmTriggerType TriggerType { get; set; }

        [Required]
        public string RecordOperations { get; set; }

        [Required]
        public WorkflowFrequency Frequency { get; set; }

        [Required]
        public WorkflowProcessFilter ProcessFilter { get; set;}

        [MaxLength(4000)]
        public string Changed_Field { get; set; }

        public bool CanStartManuel { get; set; }
        
        public JObject DefinitionJson { get; set; }

        public string DiagramJson { get; set; }

        public int ModuleId { get; set; }

        public List<BpmRecordFilter> Filters { get; set; }



    }
}