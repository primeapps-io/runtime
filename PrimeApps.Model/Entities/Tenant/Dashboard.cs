using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("dashboard")]
    public class Dashboard : BaseEntity
    {
        [Column("name_en"), MaxLength(50)]
        public string NameEn { get; set; }

        [Column("name_tr"), MaxLength(50)]
        public string NameTr { get; set; }

        [Column("description_en"), MaxLength(250)]
        public string DescriptionEn { get; set; }

        [Column("description_tr"), MaxLength(250)]
        public string DescriptionTr { get; set; }

        [Column("user_id"), ForeignKey("User")]
        public int? UserId { get; set; }

        [Column("profile_id"), ForeignKey("Profile")]
        public int? ProfileId { get; set; }

        [Column("is_active")] public bool IsActive { get; set; }

        [Column("sharing_type")] public DashboardSharingType SharingType { get; set; }

        public virtual TenantUser User { get; set; }

        public virtual Profile Profile { get; set; }
    }
}