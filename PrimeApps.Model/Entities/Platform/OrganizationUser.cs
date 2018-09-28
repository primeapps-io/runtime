using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("organization_users")]
	public class OrganizationUser
	{
		[Column("user_id")]
		public int UserId { get; set; }

		[Column("organization_id")]
		public int OrganizationId { get; set; }

	    public virtual PlatformUser PlatformUser { get; set; }

		public virtual Organization Organization { get; set; }
	}
}
