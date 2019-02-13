using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;

namespace PrimeApps.Studio.Controllers
{
	public class ChangePasswordBindingModel
	{
		[Required]
		public string Email { get; set; }

		[Required]
		[DataType(DataType.Password)]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[Display(Name = "Current password")]
		[JsonProperty("old_password")]
		public string OldPassword { get; set; }

		[Required]
		[StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
		[DataType(DataType.Password)]
		[Display(Name = "New password")]
		[JsonProperty("new_password")]
		public string NewPassword { get; set; }

		[DataType(DataType.Password)]
		[Display(Name = "Confirm new password")]
		[Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
		[JsonProperty("confirm_password")]
		public string ConfirmPassword { get; set; }
	}
}