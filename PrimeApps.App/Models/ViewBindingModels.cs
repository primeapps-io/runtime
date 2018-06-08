using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Common.Annotations;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Enums;

namespace PrimeApps.App.Models
{
    public class ViewBindingModel
    {
        [Required, Range(1, int.MaxValue)]
        public int ModuleId { get; set; }

        [Required, StringLength(50)]
        public string Label { get; set; }

        public ViewSharingType SharingType { get; set; }

        [StringLength(200), BalancedParentheses, FilterLogic]
        public string FilterLogic { get; set; }

        [RequiredCollection]
        public List<ViewFieldBindingModel> Fields { get; set; }

        public List<Filter> Filters { get; set; }

        public List<int> Shares { get; set; }
    }

    public class ViewFieldBindingModel
    {
        [Required, StringLength(120)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field { get; set; }

        [Required]
        public int Order { get; set; }
    }

    public class ViewStateBindingModel
    {
        public int? Id { get; set; }

        [Required, Range(1, int.MaxValue)]
        public int ModuleId { get; set; }

        [Required]
        public int ActiveView { get; set; }

        [StringLength(120)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string SortField { get; set; }

        public SortDirection SortDirection { get; set; }

        public int? RowPerPage { get; set; }
    }
}