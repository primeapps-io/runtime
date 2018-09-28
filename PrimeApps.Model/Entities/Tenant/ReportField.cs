using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("report_fields")]
    public class ReportField : BaseEntity
    {
        [Column("report_id"), ForeignKey("Report")]
        public int ReportId { get; set; }

        [Column("field"), MaxLength(120), Required]
        public string Field { get; set; }

        [Column("order"), Required]
        public int Order { get; set; }

        public virtual Report Report { get; set; }
    }
}