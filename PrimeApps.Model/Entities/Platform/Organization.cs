using Newtonsoft.Json;
using PrimeApps.Model.Entities.Application;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("organizations")]
	public class Organization : BaseEntity
    {
		[Column("name"), MaxLength(700)]
		public string Name { get; set; }

		[Column("owner")]
		public int Owner { get; set; }

		//Apps and Tenants One to Many 
		public virtual ICollection<Team> Teams { get; set; }

		//[JsonIgnore]
		public virtual ICollection<OrganizationUsers> Users { get; set; }

	}
}
