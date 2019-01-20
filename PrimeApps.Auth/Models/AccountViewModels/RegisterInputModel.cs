using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;
using PrimeApps.Model.Common.App;

namespace PrimeApps.Auth.UI
{
	public class RegisterInputModel : ApplicationViewModel
    {
		[DataMember(Name = "email"), Display(Name = "Email"), EmailAddress, Required]
		public string Email { get; set; }
		
		[DataMember(Name = "send_activation")]
		public bool SendActivation { get; set; }
		
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} and at max {1} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[DataMember(Name = "password"), Display(Name = "Password")]
		public string Password { get; set; }
		
		[DataMember(Name = "culture")]
		public string Culture { get; set; }

		[DataMember(Name = "tenant_id")]
		public int TenantId { get; set; }
		/*[DataType(DataType.Password)]
		[Display(Name = "Confirm password")]
		[Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
		public string ConfirmPassword { get; set; }*/

		[DataMember(Name = "first_name")]
		public string FirstName { get; set; }

		[DataMember(Name = "last_name")]
		public string LastName { get; set; }

        [DataMember(Name = "phone_number")]
        public string PhoneNumber { get; set; }

        public bool ReadOnly { get; set; }

        public bool ExternalLogin { get; set; }
    }
}
