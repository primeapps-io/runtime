using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("teams")]
	public class Team : BaseEntity
	{
		[Column("organization_id")]
		public int OrganizationId { get; set; }

		[Column("name"), MaxLength(700)]
		public string Name { get; set; }

		public virtual Organization Organization { get; set; }

		public virtual ICollection<TeamApp> TeamApps { get; set; }

		public virtual ICollection<TeamUser> TeamUsers { get; set; }
	}
}
