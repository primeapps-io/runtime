using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Application
{
    [Table("processes")]
    public class Process : BaseEntity
    {
        public Process()
        {
            _profileList = new List<string>();
        }

        [Column("module_id"), ForeignKey("Module"), Index]
        public int ModuleId { get; set; }

        [Column("user_id")]
        public int UserId { get; set; }

        [Column("name"), MaxLength(200), Required]
        public string Name { get; set; }

        [Column("frequency"), Required]
        public WorkflowFrequency Frequency { get; set; }

        [Column("approver_type"), Required]
        public ProcessApproverType ApproverType { get; set; }

        [Column("trigger_time"), Required]
        public ProcessTriggerTime TriggerTime { get; set; }

        [Column("approver_field")]
        public string ApproverField { get; set; }

        [Column("active"), Index]
        public bool Active { get; set; }

        [Column("operations"), MaxLength(50), Required]
        public string Operations { get; set; }

        [Column("profiles")]
        public string Profiles
        {
            get
            {
                return string.Join(",", _profileList);
            }
            set
            {
                _profileList = value?.Trim().Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries).ToList() ?? new List<string>();
            }
        }

        public virtual Module Module { get; set; }

        public virtual ICollection<ProcessFilter> Filters { get; set; }

        public virtual ICollection<ProcessApprover> Approvers { get; set; }

        public virtual ICollection<ProcessLog> Logs { get; set; }

        public virtual ICollection<ProcessRequest> Requests { get; set; }

        [NotMapped]
        public string[] OperationsArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(Operations))
                    return null;

                return Operations.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    Operations = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }

        [NotMapped]
        private List<string> _profileList { get; set; }

        [NotMapped]
        public List<string> ProfileList
        {
            get
            {
                return _profileList;
            }
            set
            {
                _profileList = value;
            }
        }
    }
}
