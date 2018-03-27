using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Chart
{
    public class ChartView
    {
        public int Id { get; set; }
        public ChartType ChartType { get; set; }

        public string Caption { get; set; }

        public string Subcaption { get; set; }

        public ChartTheme Theme { get; set; }

        public string Xaxisname { get; set; }

        public string Yaxisname { get; set; }

        public string ReportId { get; set; }

        public int ReportModuleId { get; set; }

        public string ReportGroupField { get; set; }

        public string ReportAggregationField { get; set; }
    }
}
