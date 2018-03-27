using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;

namespace PrimeApps.Model.Entities.Platform.Identity
{
    public class PlatformUser : IdentityUser<int>
    {
        /// <summary>
        /// User ID
        /// </summary>
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public override int Id { get; set; }

        [Column("app_id"), ForeignKey("App"), Required]//, Index]
        public int AppId { get; set; }

        [Column("tenant_id"), ForeignKey("Tenant")]//, Index]
        public int? TenantId { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [Column("email"), Required]//, Index]
        public override string Email { get; set; }

        [Column("first_name"), Required]//, Index]
        public string FirstName { get; set; }

        [Column("last_name"), Required]//, Index]
        public string LastName { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("email_confirmed")]//, Index]
        public override bool EmailConfirmed { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("password_hash")]//, Index]
        public override string PasswordHash { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("security_stamp")]//, Index]
        public override string SecurityStamp { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("phone_number")]//, Index]
        public override string PhoneNumber { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("phone_number_confirmed")]//, Index]
        public override bool PhoneNumberConfirmed { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("two_factor_enabled")]//, Index]
        public override bool TwoFactorEnabled { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("lockout_end_date_utc")]//, Index]
        public override DateTimeOffset? LockoutEnd { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("lockout_enabled")]//, Index]
        public override bool LockoutEnabled { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("access_failed_count")]//, Index]
        public override int AccessFailedCount { get; set; }

        /// <summary>
        /// Asp.Net Identity Field
        /// </summary>
        [Column("user_name")]//, Index]
        public override string UserName { get; set; }

        [Column("culture")]
        public string Culture { get; set; }

        [Column("currency")]//, Index]
        public string Currency { get; set; }
        
        [Column("created_at")]//, Index]
        public DateTime CreatedAt { get; set; }

        [Column("active_directory_tenant_id")]//, Index]
        public int ActiveDirectoryTenantId { get; set; }

        [Column("active_directory_email")]//, Index]
        public string ActiveDirectoryEmail { get; set; }

        public virtual App App { get; set; }

        public virtual Tenant Tenant { get; set; }

        //public async Task<ClaimsIdentity> GenerateUserIdentityAsync(UserManager<PlatformUser, int manager, string authenticationType)
        //{
        //    // Note the authenticationType must match the one defined in CookieAuthenticationOptions.AuthenticationType
        //    var userIdentity = await manager.CreateIdentityAsync(this, authenticationType);
        //    // Add custom user claims here
        //    return userIdentity;
        //}

        /// <summary>
        /// Returns a combination of the first name and the last name of user.
        /// </summary>
        /// <returns></returns>
        public string GetFullName()
        {
            return $"{FirstName} {LastName}";
        }
    }

}
