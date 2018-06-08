using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OfisimCRM.Model.Entities
{
    [Table("menu_items")]
    public class MenuItem : BaseEntity
    {
        [Column("menu_id"), ForeignKey("Menu"), Index]
        public int MenuId { get; set; }

        [Column("module_id"), ForeignKey("Module"), Index]
        public int? ModuleId { get; set; }

        [Column("parent_id")]
        public int? ParentId { get; set; }

        [Column("route"), MaxLength(100)]
        public string Route { get; set; }

        [Column("label_en"), MaxLength(50), Required]
        public string LabelEn { get; set; }

        [Column("label_tr"), MaxLength(50), Required]
        public string LabelTr { get; set; }

        [Column("menu_icon"), MaxLength(100)]
        public string MenuIcon { get; set; }

        [Column("order")]
        public short Order { get; set; }

        [Column("is_dynamic")]
        public bool IsDynamic { get; set; }

        public virtual MenuItem Parent { get; set; }

        public virtual Menu Menu { get; set; }

        public virtual Module Module { get; set; }

        public virtual ICollection<MenuItem> MenuItems { get; set; }
    }
}
