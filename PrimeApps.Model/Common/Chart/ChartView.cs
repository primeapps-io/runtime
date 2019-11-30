using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Chart
{
    public class ChartView
    {
        public int Id { get; set; }
        public ChartType ChartType { get; set; }

        public string CaptionEn { get; set; }
        public string CaptionTr { get; set; }

        public string SubcaptionEn { get; set; }
        public string SubcaptionTr { get; set; }

        public ChartTheme Theme { get; set; }

        public string XaxisnameEn { get; set; }
        public string XaxisnameTr { get; set; }

        public string YaxisnameEn { get; set; }
        public string YaxisnameTr { get; set; }

        public string ReportId { get; set; }

        public int ReportModuleId { get; set; }

        public string ReportGroupField { get; set; }

        public string ReportAggregationField { get; set; }
    }
}
