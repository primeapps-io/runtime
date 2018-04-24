using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("user_tenants")]
	public class UserTenant
    {
		[Column("user_id")]//]//, Index]
		public int UserId { get; set; }

		public virtual PlatformUser PlatformUser { get; set; }

		[Column("tenant_id")]//]//, Index]
		public int TenantId { get; set; }

		public virtual Tenant Tenant { get; set; }
	}
}
