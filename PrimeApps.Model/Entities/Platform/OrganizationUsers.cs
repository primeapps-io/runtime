using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("organization_users")]
	public class OrganizationUsers
	{
		[Column("user_id")]
		public int UserId { get; set; }
		public virtual PlatformUser PlatformUser { get; set; }

		[Column("organization_id")]
		public int OrganizationId { get; set; }
		public virtual Organization Organization { get; set; }

	}
}
