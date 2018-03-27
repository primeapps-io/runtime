using System.Collections.Generic;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Report
{
    public class ReportViewModel
    {
        public int Id { get; set; }
        public int ModuleId { get; set; }
        public ReportType ReportType { get; set; }
        public int? CategoryId { get; set; }
        public string GroupField { get; set; }
        public string SortField { get; set; }
        public SortDirection SortDirection { get; set; }
        public ReportSharingType SharingType { get; set; }
        public string FilterLogic { get; set; }
        public int CreatedBy { get; set; }
        public List<ReportFieldViewModel> Fields { get; set; }
        public List<ReportFilterViewModel> Filters { get; set; }
        public List<ReportAggregationViewModel> Aggregations { get; set; }
        public List<UserBasicViewModel> Shares { get; set; }
    }
}