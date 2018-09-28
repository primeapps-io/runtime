using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("dashlets")]
    public class Dashlet : BaseEntity
    {
        [Column("name"), MaxLength(50), Required]
        public string Name { get; set; }

        [Column("dashlet_area"), Required]
        public DashletArea DashletArea { get; set; }

        [Column("dashlet_type"), Required]
        public DashletType DashletType { get; set; }

        [Column("chart_id"), ForeignKey("Chart")]
        public int? ChartId { get; set; }

        [Column("widget_id"), ForeignKey("Widget")]
        public int? WidgetId { get; set; }

        [Column("order"), Required]
        public int Order { get; set; }

        [Column("x_tile_height"), Required]

        public int XTileHeight { get; set; }

        [Column("y_tile_length"), Required]

        public int YTileLength { get; set; }

        [Column("dashboard_id"), ForeignKey("Dashboard")]
        public int? DashboardId { get; set; }

        public virtual Chart Chart { get; set; }

        public virtual Widget Widget { get; set; }

        public virtual Dashboard Dashboard { get; set; }
    }
}
