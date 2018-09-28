using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("view_states")]
    public class ViewState : BaseEntity
    {
        [Column("module_id"), ForeignKey("Module")]
        public int ModuleId { get; set; }

        [Column("user_id"), ForeignKey("User")]
        public int UserId { get; set; }

        [Column("active_view"), Required]
        public int ActiveView { get; set; }

        [Column("sort_field"), MaxLength(120)]
        public string SortField { get; set; }

        [Column("sort_direction")]
        public SortDirection SortDirection { get; set; }

        [Column("row_per_page")]
        public int? RowPerPage { get; set; }

        public virtual Module Module { get; set; }

        public virtual TenantUser User { get; set; }
    }
}
