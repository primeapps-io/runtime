using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Common.Annotations;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Enums;

namespace PrimeApps.Studio.Models
{
    public class ReportBindingModel
    {
        [StringLength(100)]
        public string NameEn { get; set; }
        [StringLength(100)]
        public string NameTr { get; set; }

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

        [StringLength(200), BalancedParentheses, FilterLogic]
        public string FilterLogic { get; set; }

        public ChartBindingModel Chart { get; set; }

        public WidgetBindingModel Widget { get; set; }

        public List<ReportFieldBindingModel> Fields { get; set; }

        public List<Filter> Filters { get; set; }

        public List<ReportAggregationBindingModel> Aggregations { get; set; }

        public List<int> Shares { get; set; }

        public SystemType SystemType { get; set; }
    }

    public class ReportFieldBindingModel
    {
        [Required, StringLength(120)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field { get; set; }

        [Required]
        public int Order { get; set; }
    }

    public class ReportAggregationBindingModel
    {
        [Required, StringLength(120)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field { get; set; }

        [Required]
        public AggregationType AggregationType { get; set; }
    }

    public class ReportCategoryBindingModel
    {
        [StringLength(100)]
        public string NameEn { get; set; }    
        [StringLength(100)]
        public string NameTr { get; set; }

        public int Order { get; set; }

        public int? UserId { get; set; }
    }

    public class ChartBindingModel
    {
        [Required]
        public ChartType Type { get; set; }

        [StringLength(100)]
        public string CaptionEn { get; set; }
        [StringLength(100)]
        public string CaptionTr { get; set; }

        [StringLength(200)]
        public string SubCaptionEn { get; set; }    
        [StringLength(200)]
        public string SubCaptionTr { get; set; }

        public string XaxisNameEn { get; set; }
        public string XaxisNameTr { get; set; }

        public string YaxisNameEn { get; set; }
        public string YaxisNameTr { get; set; }

        public ChartTheme Theme { get; set; }
    }

    public class WidgetBindingModel
    {
        [Required]
        public WidgetType WidgetType { get; set; }

        [Required, StringLength(100)]
        public string NameEn { get; set; }   
        [Required, StringLength(100)]
        public string NameTr { get; set; }

        public string Color { get; set; }

        public string Icon { get; set; }
    }
}