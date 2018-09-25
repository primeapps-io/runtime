using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("charts")]
    public class Chart : BaseEntity
    {
        [Column("chart_type")]
        public ChartType ChartType { get; set; }

        [Column("caption"), MaxLength(100), Required]
        public string Caption { get; set; }

        [Column("sub_caption"), MaxLength(200)]
        public string SubCaption { get; set; }

        [Column("theme")]
        public ChartTheme Theme { get; set; }

        [Column("x_axis_name"), MaxLength(80), Required]
        public string XaxisName { get; set; }

        [Column("y_axis_name"), MaxLength(80), Required]
        public string YaxisName { get; set; }

        [Column("report_id"), ForeignKey("Report")]//, Index]
        public int? ReportId { get; set; }

        public virtual Report Report { get; set; }
    }
}
