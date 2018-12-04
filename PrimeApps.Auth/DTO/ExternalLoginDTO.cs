using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Auth.DTO
{
    public class ExternalLoginDTO
    {
        public string client { get; set; }
        public string email { get; set; }
        public string password { get; set; }
    }
}
