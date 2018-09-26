using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("report_categories")]
    public class ReportCategory : BaseEntity
    {
        [Column("name"), MaxLength(100), Required]
        public string Name { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("user_id"), ForeignKey("User")]//, Index]

        public int? UserId { get; set; }

        public virtual TenantUser User { get; set; }
    }
}