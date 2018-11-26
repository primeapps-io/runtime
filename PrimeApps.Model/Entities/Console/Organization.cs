using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Console
{
    [Table("organizations")]
    public class Organization : BaseEntity
    {
        [Column("name"), Required, MaxLength(50)]
        public string Name { get; set; }

        [Column("icon"), MaxLength(200)]
        public string Icon { get; set; }

        [Column("owner_id"), Required, ForeignKey("Owner")]
        public int OwnerId { get; set; }

        public virtual ConsoleUser Owner { get; set; }

        public virtual ICollection<Team> Teams { get; set; }

        public virtual ICollection<OrganizationUser> OrganizationUsers { get; set; }
    }
}
