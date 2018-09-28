using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("users")]
    public class PlatformUser
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("email"), Required]
        public string Email { get; set; }

        [Column("first_name"), Required]
        public string FirstName { get; set; }

        [Column("last_name"), Required]
        public string LastName { get; set; }

        [Column("created_at"), Required]
        public DateTime CreatedAt { get; set; }

        [Column("updated_at")]
        public DateTime? UpdatedAt { get; set; }

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
