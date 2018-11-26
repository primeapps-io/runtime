using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("user_tenants")]
    public class UserTenant
    {
        [Column("user_id")]
        public int UserId { get; set; }

        [Column("tenant_id")]
        public int TenantId { get; set; }

        public virtual PlatformUser PlatformUser { get; set; }

        public virtual Tenant Tenant { get; set; }
    }
}
