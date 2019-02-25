using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Studio
{
    [Table("teams")]
    public class Team : BaseEntity
    {
        [Column("organization_id"), ForeignKey("Organization")]
        public int OrganizationId { get; set; }

        [Column("name"), Required, MaxLength(50)]
        public string Name { get; set; }

        [Column("icon"), MaxLength(200)]
        public string Icon { get; set; }

        public virtual Organization Organization { get; set; }

        public virtual ICollection<TeamUser> TeamUsers { get; set; }
    }
}
