using System.Security.Claims;
using System.Threading.Tasks;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("users")]
	public class PlatformUser
    {
        /// <summary>
        /// User ID
        /// </summary>
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

		//TODO Removed
		/*[Column("app_id"), ForeignKey("App"), Required]//]//, Index] 
        public int AppId { get; set; }

        [Column("tenant_id"), ForeignKey("Tenant")]//]//, Index]
        public int? TenantId { get; set; }*/

        /// <summary>
        /// Email address
        /// </summary>
        [Column("email"), Required]//]//, Index]
        public string Email { get; set; }

        [Column("first_name"), Required]//]//, Index]
        public string FirstName { get; set; }

        [Column("last_name"), Required]//]//, Index]
        public string LastName { get; set; }

		[Column("created_at")]//]//, Index]
        public DateTime CreatedAt { get; set; }

		[Column("updated_at")]//]//, Index]
		public DateTime UpdatedAt { get; set; }

		//TODO Removed
		/*[Column("active_directory_tenant_id")]//]//, Index]
        public int ActiveDirectoryTenantId { get; set; }

        [Column("active_directory_email")]//]//, Index]
        public string ActiveDirectoryEmail { get; set; }*/

		//TODO Removed
		///public virtual App App { get; set; }

		//TODO Removed
		//public virtual Tenant Tenant { get; set; }
		//public virtual ICollection<Tenant> Tenant { get; set; }

		[JsonIgnore]
		public virtual ICollection<TeamUser> UserTeams { get; set; }

		[JsonIgnore]
		public virtual ICollection<OrganizationUser> UserOrganizations { get; set; }

		
		public virtual ICollection<UserTenant> TenantsAsUser { get; set; }

		public virtual ICollection<Tenant> TenantsAsOwner { get; set; }

		public virtual PlatformUserSetting Setting { get; set; }


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
