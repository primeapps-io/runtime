using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Application
{
    [Table("bpm_workflows")]
    public class BpmWorkflow : BaseEntity
    {
        [Column("code"), MaxLength(200), Required]
        public string Code { get; set; }

        [Column("name"), MaxLength(100), Required]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("category_id"), ForeignKey("Category")]
        public int? CategoryId { get; set; }

        [Column("version"), Required]
        public int Version { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("trigger_type"), Required]
        public BpmTriggerType TriggerType { get; set; }

        [Column("start_time")]
        public DateTime StartTime { get; set; }

        [Column("end_time")]
        public DateTime EndTime { get; set; }

        [Column("record_operations"), MaxLength(50), Required]
        public string RecordOperations { get; set; }

        [Column("frequency"), Required]
        public WorkflowFrequency Frequency { get; set; }

        [Column("changed_fields"), MaxLength(4000)]
        public string ChangedFields { get; set; }

        [Column("can_start_manuel")]
        public bool CanStartManuel { get; set; }

        [Column("definition_json")]
        public string DefinitionJson { get; set; }

        [Column("diagram_json")]
        public string DiagramJson { get; set; }

        [Column("module_id"), Range(1, int.MaxValue)]
        public int ModuleId { get; set; }

        public virtual BpmCategory Category { get; set; }

        public virtual ICollection<BpmRecordFilter> Filters { get; set; }

        [NotMapped]
        public string[] OperationsArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(RecordOperations))
                    return null;

                return RecordOperations.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    RecordOperations = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }

        [NotMapped]
        public string[] ChangedFieldsArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ChangedFields))
                    return null;

                return ChangedFields.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    ChangedFields = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }
    }
}