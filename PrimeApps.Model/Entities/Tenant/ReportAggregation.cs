using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("report_aggregations")]
    public class ReportAggregation : BaseEntity
    {
        [Column("report_id"), ForeignKey("Report")]
        public int ReportId { get; set; }

        [Column("type")]
        public AggregationType AggregationType { get; set; }

        [Column("field"), MaxLength(120), Required]
        public string Field { get; set; }

        public virtual Report Report { get; set; }
    }
}