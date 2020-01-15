using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Common.Annotations;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class ProcessBindingModel
    {
        [Required, Range(1, int.MaxValue)]
        public int ModuleId { get; set; }

        [Required, Range(0, int.MaxValue)]
        public int UserId { get; set; }

        [Required, StringLength(200)]
        public string Name { get; set; }

        public string Profiles { get; set; }

        [Required]
        public WorkflowFrequency Frequency { get; set; }

        [Required]
        public ProcessApproverType ApproverType { get; set; }

        [Required]
        public ProcessTriggerTime TriggerTime { get; set; }

        public string ApproverField { get; set; }

        public bool Active { get; set; }

        [RequiredCollection]
        public string[] Operations { get; set; }

        public List<Filter> Filters { get; set; }

        public List<ApproversBindingModel> Approvers { get; set; }

        public List<EnvironmentType> Environments { get; set; }

        public string EnvironmentValues
        {
            get
            {
                var list = new List<string>();

                foreach (var env in Environments)
                {
                    var value = (int)env;
                    list.Add(value.ToString());
                }

                return string.Join(",", list);
            }

            set
            {
                var list = value.Split(",");

                foreach (var env in list)
                {
                    Environments.Add((EnvironmentType)Enum.Parse(typeof(EnvironmentType), env));
                }

            }
        }
    }

    public class ApproversBindingModel
    {
        public short Order { get; set; }

        public int UserId { get; set; }
    }
}