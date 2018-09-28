using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System.Collections.Generic;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("sections")]
    public class Section : BaseEntity
    {
        [JsonIgnore]
        [Column("module_id"), ForeignKey("Module")]
        public int ModuleId { get; set; }

        [Column("name"), MaxLength(50), Required]
        public string Name { get; set; }

        [Column("system_type"), Required]
        public SystemType SystemType { get; set; }

        [Column("order"), Required]
        public short Order { get; set; }

        [Column("column_count"), Required]
        public byte ColumnCount { get; set; }

        [Column("label_en"), MaxLength(50), Required]
        public string LabelEn { get; set; }

        [Column("label_tr"), MaxLength(50), Required]
        public string LabelTr { get; set; }

        [Column("display_form")]
        public bool DisplayForm { get; set; }

        [Column("display_detail")]
        public bool DisplayDetail { get; set; }

        [Column("custom_label"), MaxLength(1000)]
        public string CustomLabel { get; set; }

        public virtual Module Module { get; set; }

        public virtual ICollection<SectionPermission> Permissions { get; set; }
    }
}
