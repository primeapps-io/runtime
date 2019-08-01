using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;

namespace PrimeApps.App.Models
{
    // Models used as parameters to AccountController actions.

    public class LoginBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        [EmailAddress]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [Display(Name = "Remember me?")]
        public bool RememberMe { get; set; }
    }

    public class AddExternalLoginBindingModel
    {
        [Required]
        [Display(Name = "External access token")]
        public string ExternalAccessToken { get; set; }
    }

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

    public class RegisterBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        [JsonProperty("email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone")]
        public string Phone { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [JsonProperty("password")]
        public string Password { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [Display(Name = "First Name")]
        [JsonProperty("first_name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(40, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        [JsonProperty("last_name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "License")]
        public string License { get; set; }

        [Display(Name = "Culture")]
        public string Culture { get; set; }

        [Display(Name = "Currency")]
        public string Currency { get; set; }

        [Display(Name = "Campaign Code")]
        public string CampaignCode { get; set; }

        public bool OfficeSignIn { get; set; }

        [Display(Name = "AppId")]
        public int AppID { get; set; }

        public object ModuleLicenseCount { get; internal set; }
    }

    public class AutomaticAccountActivationModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public int Id { get; set; }
    }

    public class RegisterExternalBindingModel
    {
        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }
    }

    public class RemoveLoginBindingModel
    {
        [Required]
        [Display(Name = "Login provider")]
        public string LoginProvider { get; set; }

        [Required]
        [Display(Name = "Provider key")]
        public string ProviderKey { get; set; }
    }

    public class SetPasswordBindingModel
    {
        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "New password")]
        public string NewPassword { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm new password")]
        [Compare("NewPassword", ErrorMessage = "The new password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }
    }

    public class ResetPasswordBindingModel
    {
        [Required]
        [Display(Name = "UserId")]
        public int UserId { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        public string Token { get; set; }
    }

    //TODO Removed
    /*public class ClientBindingModel
    {
        public string Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public ApplicationTypesEnum ApplicationType { get; set; }

        public int RefreshTokenLifeTime { get; set; }

        [MaxLength(200)]
        public string AllowedOrigin { get; set; }
    }*/

    public class ChangeEmailBindingModel
    {
        [Required]
        [Display(Name = "CurrentEmail")]
        public string CurrentEmail { get; set; }

        [Required]
        [Display(Name = "NewEmail")]
        public string NewEmail { get; set; }
    }

    public class AddUserBindingModel
    {
        [Required, StringLength(100), JsonProperty("email")]
        public string Email { get; set; }

        [Required, StringLength(40), JsonProperty("first_name")]
        public string FirstName { get; set; }

        [Required, StringLength(40), JsonProperty("last_name")]
        public string LastName { get; set; }

        [Required, StringLength(100), JsonProperty("phone")]
        public string Phone { get; set; }

        [Required, JsonProperty("profile_id")]
        public int ProfileId { get; set; }

        [Required, JsonProperty("role_id")]
        public int RoleId { get; set; }

        [Required, JsonProperty("app_id")]
        public int AppId { get; set; }

        [Required, JsonProperty("tenant_id")]
        public int TenantId { get; set; }

        [Required, JsonProperty("dont_send_mail")]
        public bool DontSendMail { get; set; }
    }
}