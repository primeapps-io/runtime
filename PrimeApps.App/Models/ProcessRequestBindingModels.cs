using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Models
{
    public class ProcessRequestModel
    {
        [Required, Range(1, int.MaxValue)]
        public int RecordId { get; set; }

        [Required]
        public OperationType OperationType { get; set; }
    }

    public class ProcessRequestRejectModel : ProcessRequestModel
    {
        [Required, StringLength(500)]
        public string Message { get; set; }
    }

    public class ProcessRequestDeleteModel
    {
        [Required, Range(1, int.MaxValue)]
        public int RecordId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int ModuleId { get; set; }
    }

    public class ProcessRequestManuelModel
    {
        [Required, Range(1, int.MaxValue)]
        public int RecordId { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int ModuleId { get; set; }
    }
}