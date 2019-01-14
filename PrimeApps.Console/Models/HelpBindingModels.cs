using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Enums;
using DataType = PrimeApps.Model.Enums.DataType;

namespace PrimeApps.Console.Models
{
    public class HelpBindingModel
    {
        [Required]
        public string Template { get; set; }

        public int? ModuleId { get; set; }

        public string RouteUrl { get; set; }

        [Required]
        public string Name { get; set; }

        public ModalType ModalType { get; set; }

        public ShowType ShowType { get; set; }

        public ModuleType ModuleType { get; set; }

        public bool FirstScreen { get; set; }

        public bool CustomHelp { get; set; }

        public string HelpRelation { get; set; }

    }
}