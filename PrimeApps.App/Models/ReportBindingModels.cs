using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Common.Annotations;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Models
{
    public class ReportBindingModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int ModuleId { get; set; }

        [Required]
        public ReportType ReportType { get; set; }

        [Required]
        public int CategoryId { get; set; }

        public string GroupField { get; set; }

        public string SortField { get; set; }

        public SortDirection SortDirection { get; set; }

        public ReportSharingType SharingType { get; set; }

        [StringLength(50), BalancedParentheses, FilterLogic]
        public string FilterLogic { get; set; }

        public ChartBindingModel Chart { get; set; }

        public WidgetBindingModel Widget { get; set; }

        public List<ReportFieldBindingModel> Fields { get; set; }

        public List<Filter> Filters { get; set; }

        public List<ReportAggregationBindingModel> Aggregations { get; set; }

        public List<int> Shares { get; set; }
    }

    public class ReportFieldBindingModel
    {
        [Required, StringLength(120)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegex, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field { get; set; }

        [Required]
        public int Order { get; set; }
    }

    public class ReportAggregationBindingModel
    {
        [Required, StringLength(120)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegex, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field { get; set; }

        [Required]
        public AggregationType AggregationType { get; set; }
    }

    public class ReportCategoryBindingModel
    {
        [Required, StringLength(100)]
        public string Name { get; set; }

        public int Order { get; set; }

        public int? UserId { get; set; }
    }

    public class ChartBindingModel
    {
        [Required]
        public ChartType Type { get; set; }

        [Required, StringLength(100)]
        public string Caption { get; set; }

        [StringLength(200)]
        public string SubCaption { get; set; }

        public string XaxisName { get; set; }

        public string YaxisName { get; set; }

        public ChartTheme Theme { get; set; }
    }

    public class WidgetBindingModel
    {
        [Required]
        public WidgetType WidgetType { get; set; }

        [Required, StringLength(100)]
        public string Name { get; set; }

        public string Color { get; set; }

        public string Icon { get; set; }
    }
}