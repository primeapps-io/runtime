using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Studio.Models
{
    public class SettingBindingModel
    {

        public int? UserId;

        [Required]
        public string Key;

        [Required]
        public string Value;

        public SettingType Type;
    }
}
