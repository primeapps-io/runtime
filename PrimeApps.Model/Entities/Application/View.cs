using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Application
{
    [Table("views")]
    public class View : BaseEntity
    {
        [Column("module_id"), ForeignKey("Module")]//, Index]
        public int ModuleId { get; set; }

        [Column("system_type"), Required]
        public SystemType SystemType { get; set; }

        [Column("label_en"), MaxLength(50), Required]
        public string LabelEn { get; set; }

        [Column("label_tr"), MaxLength(50), Required]
        public string LabelTr { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("sharing_type")]//, Index]
        public ViewSharingType SharingType { get; set; }
        
        [Column("filter_logic"), MaxLength(50)]
        public string FilterLogic { get; set; }

        public virtual Module Module { get; set; }

        public ICollection<ViewField> Fields { get; set; }
               
        public ICollection<ViewFilter> Filters { get; set; }
               
        public ICollection<ViewShares> Shares { get; set; }
    }
}
