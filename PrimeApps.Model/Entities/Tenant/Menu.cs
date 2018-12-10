using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Tenant
{
	[Table("menu")]
	public class Menu : BaseEntity
	{
		[Column("name"), Required]
		public string Name { get; set; }

		[Column("profile_id"), ForeignKey("Profile"), /*Index*/]
		public int ProfileId { get; set; }

		[Column("default")]
		public bool Default { get; set; }

		[Column("description"), MaxLength(500)]
		public string Description { get; set; }

		public virtual Profile Profile { get; set; }
	}
}
