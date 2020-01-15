using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("report_categories")]
    public class ReportCategory : BaseEntity
    {
        [Column("name_en"), MaxLength(100)]
        public string NameEn { get; set; } 
        
        [Column("name_tr"), MaxLength(100)]
        public string NameTr { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("user_id"), ForeignKey("User")]

        public int? UserId { get; set; }

        public virtual TenantUser User { get; set; }
    }
}