using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Enums;
using DataType = PrimeApps.Model.Enums.DataType;

namespace PrimeApps.Console.Models
{
    public class ModuleBindingModel
    {
        [Required, StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Name { get; set; }

        [Required, StringLength(50)]
        public string LabelEnSingular { get; set; }

        [Required, StringLength(50)]
        public string LabelTrSingular { get; set; }

        [Required, StringLength(50)]
        public string LabelEnPlural { get; set; }

        [Required, StringLength(50)]
        public string LabelTrPlural { get; set; }

        [Required]
        public short Order { get; set; }

        public bool Display { get; set; }

        public Sharing Sharing { get; set; }

        [StringLength(100)]
        public string MenuIcon { get; set; }

        public bool LocationEnabled { get; set; }

        public bool DisplayCalendar { get; set; }

        [StringLength(20)]
        public string CalendarColorDark { get; set; }

        [StringLength(20)]
        public string CalendarColorLight { get; set; }

        public DetailViewType DetailViewType { get; set; }
        public List<SectionBindingModel> Sections { get; set; }

        public List<FieldBindingModel> Fields { get; set; }

        public List<RelationBindingModel> Relations { get; set; }

        public List<DependencyBindingModel> Dependencies { get; set; }

        public List<CalculationBindingModel> Calculations { get; set; }
    }

    public class SectionBindingModel
    {
        public int? Id { get; set; }

        [Required, StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Name { get; set; }

        [Required, StringLength(50)]
        public string LabelEn { get; set; }

        [Required, StringLength(50)]
        public string LabelTr { get; set; }

        public bool DisplayForm { get; set; }

        public bool DisplayDetail { get; set; }

        [Required]
        public short Order { get; set; }

        [Required]
        public byte ColumnCount { get; set; }

        public string CustomLabel { get; set; }

        public bool Deleted { get; set; }

        public List<SectionPermissionBindingModel> Permissions { get; set; }


    }

    public class FieldBindingModel
    {
        public int? Id { get; set; }

        [Required, StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Name { get; set; }

        [Required]
        public DataType DataType { get; set; }

        [Required, StringLength(50)]
        public string LabelEn { get; set; }

        [Required, StringLength(50)]
        public string LabelTr { get; set; }

        public bool DisplayList { get; set; }

        public bool DisplayForm { get; set; }

        public bool DisplayDetail { get; set; }

        [Required]
        public short Order { get; set; }

        [Required, StringLength(50)]
        public string Section { get; set; }

        [Required]
        public byte SectionColumn { get; set; }

        public bool Primary { get; set; }

        [StringLength(500)]
        public string DefaultValue { get; set; }

        public bool InlineEdit { get; set; }

        public bool ShowLabel { get; set; }

        public bool Editable { get; set; }

        public bool Encrypted { get; set; }

        public string EncryptionAuthorizedUsers { get; set; }

        public MultilineType MultilineType { get; set; }

        public bool MultilineTypeUseHtml { get; set; }

        public int? PicklistId { get; set; }

        public SortOrder PicklistSortorder { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string LookupType { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string LookupRelation { get; set; }

        public byte DecimalPlaces { get; set; }

        public Rounding Rounding { get; set; }

        [StringLength(10)]
        public string CurrencySymbol { get; set; }

        [StringLength(10)]
        public string AutoNumberPrefix { get; set; }

        [StringLength(10)]
        public string AutoNumberSuffix { get; set; }

        [StringLength(100)]
        public string Mask { get; set; }

        [StringLength(400)]
        public string Placeholder { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string UniqueCombine { get; set; }

        public AddressType AddressType { get; set; }
        public LookupSearchType LookupSearchType { get; set; }

        public bool ShowOnlyEdit { get; set; }

        public string StyleLabel { get; set; }

        public string StyleInput { get; set; }

        public CalendarDateType CalendarDateType { get; set; }

        public bool DocumentSearch { get; set; }

        public bool PrimaryLookup { get; set; }

        public bool Deleted { get; set; }
        public string CustomLabel { get; set; }

        public int ImageSizeList { get; set; }

        public int ImageSizeDetail { get; set; }

        public FieldViewType ViewType { get; set; }

        public Position Position { get; set; }

        public bool ShowAsDropdown { get; set; }
        public FieldValidationBindingModel Validation { get; set; }

        public FieldCombinationBindingModel Combination { get; set; }

        public List<FieldPermissionBindingModel> Permissions { get; set; }
    }

    public class FieldValidationBindingModel
    {
        public bool? Required { get; set; }

        public bool? Readonly { get; set; }

        public short? MinLength { get; set; }

        public short? MaxLength { get; set; }

        public double? Min { get; set; }

        public double? Max { get; set; }

        [StringLength(200)]
        public string Pattern { get; set; }

        public bool? Unique { get; set; }
        public bool? AddresType { get; set; }

        public bool Deleted { get; set; }
    }

    public class FieldCombinationBindingModel
    {
        [Required, StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field1 { get; set; }

        [Required, StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field2 { get; set; }

        [StringLength(50)]
        public string CombinationCharacter { get; set; }

        public bool Deleted { get; set; }
    }

    public class FieldPermissionBindingModel
    {
        public int? Id { get; set; }

        public int ProfileId { get; set; }

        public FieldPermissionType Type { get; set; }
    }

    public class RelationBindingModel : IValidatableObject
    {
        public int? Id { get; set; }

        [Required]
        public string RelatedModule { get; set; }

        [Required]
        public RelationType RelationType { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string RelationField { get; set; }

        public string[] DisplayFields { get; set; }

        [Required, StringLength(50)]
        public string LabelEnSingular { get; set; }

        [Required, StringLength(50)]
        public string LabelTrSingular { get; set; }

        [Required, StringLength(50)]
        public string LabelEnPlural { get; set; }

        [Required, StringLength(50)]
        public string LabelTrPlural { get; set; }

        public bool Readonly { get; set; }

        [Required]
        public short Order { get; set; }

        public bool Deleted { get; set; }

        public bool TwoWay { get; set; }

        public DetailViewType DetailViewType { get; set; }
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (RelationType == RelationType.OneToMany && string.IsNullOrWhiteSpace(RelationField))
                yield return new ValidationResult("The RelationField field is required.", new[] { "RelationField" });
        }
    }

    public class DependencyBindingModel
    {
        public int? Id { get; set; }

        [Required]
        public DependencyType DependencyType { get; set; }

        [Required, StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string ParentField { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string ChildField { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string ChildSection { get; set; }

        public string[] Values { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string FieldMapParent { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string FieldMapChild { get; set; }

        [StringLength(4000)]
        public string ValueMap { get; set; }

        public bool Otherwise { get; set; }

        public bool Clear { get; set; }

        public bool Deleted { get; set; }
    }

    public class CalculationBindingModel
    {
        public int? Id { get; set; }

        [Required, StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string ResultField { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field1 { get; set; }

        [StringLength(50)]
        [RegularExpression(AlphanumericConstants.AlphanumericUnderscoreRegexForField, ErrorMessage = ValidationMessages.AlphanumericError)]
        public string Field2 { get; set; }

        public double? CustomValue { get; set; }

        [Required, StringLength(1)]
        public string Operator { get; set; }

        [Required]
        public short Order { get; set; }

        public bool Deleted { get; set; }
    }

    public class SectionPermissionBindingModel
    {
        public int? Id { get; set; }

        public int ProfileId { get; set; }

        public SectionPermissionType Type { get; set; }
    }
}