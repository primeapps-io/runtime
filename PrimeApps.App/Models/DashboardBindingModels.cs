using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Models
{
    public class DashboardBindingModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [StringLength(250)]
        public string Description { get; set; }

        public int? UserId { get; set; }

        public int? ProfileId { get; set; }

        public DashboardSharingType SharingType { get; set; }


    }
    public class DashletBindingModel
    {
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public DashletArea DashletArea { get; set; }

        [Required]
        public DashletType DashletType { get; set; }

        public int? ChartId { get; set; }

        public int? WidgetId { get; set; }

        public int XTileHeight { get; set; }

        public int YTileLength { get; set; }

        [Required]
        public int Order { get; set; }

        public int? DashboardId { get; set; }

        public int? ViewId { get; set; }

        public string Color { get; set; }
        public string Icon { get; set; }

    }

}