using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace PrimeApps.Auth.UI
{
    public class AddUserBindingModel
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

        [DataMember(Name = "app_id")]
        public int AppId { get; set; }

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
        //public string PhoneNumber { get; set; }

        public bool ReadOnly { get; set; }

        [DataMember(Name = "profile_id"), Required]
        public int ProfileId { get; set; }

        [DataMember(Name = "role_id"), Required]
        public int RoleId { get; set; }

        [DataMember(Name = "phone")]
        public string Phone { get; set; } 
        
        [DataMember(Name = "language")]
        public string Language { get; set; }
        
        [DataMember(Name = "currency")]
        public string Currency { get; set; }  
    }
}
