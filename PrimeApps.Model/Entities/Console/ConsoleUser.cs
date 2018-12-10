using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Console
{
    [Table("users")]
    public class ConsoleUser
    {
        [Column("id"), Key, Required, DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public virtual ICollection<TeamUser> UserTeams { get; set; }

        public virtual ICollection<OrganizationUser> UserOrganizations { get; set; }
    }
}
