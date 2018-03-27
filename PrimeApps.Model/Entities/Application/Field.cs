using System;
using System.Collections.Generic;
using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Application
{
    [Table("fields")]
    public class Field : BaseEntity
    {
        [JsonIgnore]
        [Column("module_id"), ForeignKey("Module"), /*Index("fields_IX_module_id_name", 1, IsUnique = true)*/]
        public int ModuleId { get; set; }

        [Column("name"), Required, MaxLength(50), /*Index("fields_IX_module_id_name", 2, IsUnique = true)*/]
        public string Name { get; set; }

        [Column("system_type"), Required]
        public SystemType SystemType { get; set; }

        [Column("data_type"), Required]
        public Enums.DataType DataType { get; set; }

        [Column("order"), Required]
        public short Order { get; set; }

        [Column("section"), MaxLength(50)]
        public string Section { get; set; }

        [Column("section_column"), Required]
        public byte SectionColumn { get; set; }

        [Column("primary")]
        public bool Primary { get; set; }

        [Column("default_value"), MaxLength(500)]
        public string DefaultValue { get; set; }

        [Column("inline_edit")]
        public bool InlineEdit { get; set; }

        [Column("editable")]
        public bool Editable { get; set; }

        [Column("show_label")]
        public bool ShowLabel { get; set; }

        [Column("multiline_type")]
        public MultilineType MultilineType { get; set; }

        [Column("multiline_type_use_html")]
        public bool MultilineTypeUseHtml { get; set; }

        [Column("picklist_id"), ForeignKey("Picklist")]
        public int? PicklistId { get; set; }

        [Column("picklist_sortorder")]
        public SortOrder PicklistSortorder { get; set; }

        [Column("lookup_type"), MaxLength(50)]
        public string LookupType { get; set; }

        [Column("lookup_relation"), MaxLength(50)]
        public string LookupRelation { get; set; }

        [Column("decimal_places")]
        public byte DecimalPlaces { get; set; }

        [Column("rounding")]
        public Rounding Rounding { get; set; }

        [Column("currency_symbol"), MaxLength(10)]
        public string CurrencySymbol { get; set; }

        [Column("auto_number_prefix"), MaxLength(10)]
        public string AutoNumberPrefix { get; set; }

        [Column("auto_number_suffix"), MaxLength(10)]
        public string AutoNumberSuffix { get; set; }

        [Column("mask"), MaxLength(100)]
        public string Mask { get; set; }

        [Column("placeholder"), MaxLength(400)]
        public string Placeholder { get; set; }

        [Column("unique_combine"), MaxLength(50)]
        public string UniqueCombine { get; set; }

        [Column("address_type")]
        public AddressType AddressType { get; set; }

        [Column("label_en"), MaxLength(50), Required]
        public string LabelEn { get; set; }

        [Column("label_tr"), MaxLength(50), Required]
        public string LabelTr { get; set; }

        [Column("display_list")]
        public bool DisplayList { get; set; }

        [Column("display_form")]
        public bool DisplayForm { get; set; }

        [Column("display_detail")]
        public bool DisplayDetail { get; set; }

        [Column("show_only_edit")]
        public bool ShowOnlyEdit { get; set; }

        [Column("style_label"), MaxLength(400)]
        public string StyleLabel { get; set; }

        [Column("style_input"), MaxLength(400)]
        public string StyleInput { get; set; }

        [Column("calendar_date_type")]
        public CalendarDateType CalendarDateType { get; set; }

        [Column("document_search")]
        public bool DocumentSearch { get; set; }

        [Column("primary_lookup")]
        public bool PrimaryLookup { get; set; }

        [Column("custom_label"), MaxLength(1000)]
        public string CustomLabel { get; set; }

        [Column("image_size_list")]
        public int ImageSizeList { get; set; }

        [Column("image_size_detail")]
        public int ImageSizeDetail { get; set; }

        [Column("view_type")]
        public FieldViewType ViewType { get; set; }

        [Column("position")]
        public Position Position { get; set; }

        [Column("show_as_dropdown")]
        public bool ShowAsDropdown { get; set; }
        public virtual FieldValidation Validation { get; set; }

        public virtual FieldCombination Combination { get; set; }

        public virtual ICollection<FieldFilter> Filters { get; set; }

        public virtual Module Module { get; set; }

        public virtual Picklist Picklist { get; set; }

        public virtual ICollection<FieldPermission> Permissions { get; set; }
    }
}
