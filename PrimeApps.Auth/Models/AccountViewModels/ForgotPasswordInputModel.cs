using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Common.App;

namespace PrimeApps.Auth.UI
{
    public class ForgotPasswordInputModel : ApplicationViewModel
    {
        [Required]
        public string Email { get; set; }
    }
}
