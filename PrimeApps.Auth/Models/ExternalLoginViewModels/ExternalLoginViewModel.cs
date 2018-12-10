using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.UI
{
    public class ExternalLoginBindingModel
    {
        public string client { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}
