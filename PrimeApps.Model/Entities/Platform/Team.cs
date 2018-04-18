using Newtonsoft.Json;
using PrimeApps.Model.Entities.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("teams")]
	public class Team : BaseEntity
	{
		[Column("organization_id")]
		public int OrganizationId { get; set; }

		[Column("name"), MaxLength(700)]
		public string Name { get; set; }

		[Column("owner")]
		public int Owner { get; set; }

		//Organization and Team One to Many 
		public virtual Organization Organization { get; set; }

		//[JsonIgnore]
		public virtual ICollection<TeamApps> Apps { get; set; }

		//[JsonIgnore]
		public virtual ICollection<TeamUsers> Users { get; set; }

	}
}
