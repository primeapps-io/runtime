using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("organizations")]
    public class Organization : BaseEntity
    {
        [Column("name"), MaxLength(700)]
        public string Name { get; set; }

        [Column("label"), MaxLength(50)]
        public string Label { get; set; }

        [Column("owner_id"), Required, ForeignKey("Owner")]
        public int OwnerId { get; set; }

        public virtual PlatformUser Owner { get; set; }

        public virtual ICollection<Team> Teams { get; set; }

        public virtual ICollection<OrganizationUser> OrganizationUsers { get; set; }
    }
}
