using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrimeApps.App.Models
{
    public class UserCustomShareBindingModels
    {
        public int UserId { get; set; }

        public int SharedUserId { get; set; }

        public string Users { get; set; }

        public string UserGroups { get; set; }

        public string Modules { get; set; }
    }
}