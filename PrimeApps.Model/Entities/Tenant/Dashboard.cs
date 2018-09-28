using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("dashboard")]
    public class Dashboard : BaseEntity
    {
        [Column("name"), MaxLength(50), Required]
        public string Name { get; set; }

        [Column("description"), MaxLength(250)]
        public string Description { get; set; }

        [Column("user_id"), ForeignKey("User")]
        public int? UserId { get; set; }

        [Column("profile_id"), ForeignKey("Profile")]
        public int? ProfileId { get; set; }

        [Column("is_active")]
        public bool IsActive { get; set; }

        [Column("sharing_type")]
        public DashboardSharingType SharingType { get; set; }

        public virtual TenantUser User { get; set; }

        public virtual Profile Profile { get; set; }
    }
}
