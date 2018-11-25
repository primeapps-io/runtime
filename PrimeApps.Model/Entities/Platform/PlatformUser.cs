using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

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

        public virtual ICollection<UserTenant> TenantsAsUser { get; set; }

        public virtual ICollection<Tenant> TenantsAsOwner { get; set; }

        public virtual PlatformUserSetting Setting { get; set; }

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
