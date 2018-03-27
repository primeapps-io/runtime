using PrimeApps.Model.Common.Chart;
using PrimeApps.Model.Common.Widget;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Dashlet
{
    public class DashletView
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int XTileHeight { get; set; }

        public int YTileLength { get; set; }

        public DashletType DashletType { get; set; }

        public string DataResource { get; set; }

        public ChartItem ChartItem { get; set; }

        public WidgetView Widget { get; set; }

        public int Order { get; set; }
    }
}
