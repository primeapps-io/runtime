using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace PrimeApps.App.Models
{
    public class DocumentBindingModels
    {
        public int ModuleId { get; set; }

        public int RecordId { get; set; }

        public int UserId { get; set; }

        public int TenantId { get; set; }

        public int ContainerId { get; set; }

    }
}