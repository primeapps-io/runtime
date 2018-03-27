using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Application
{
    [Table("reports")]
    public class Report : BaseEntity
    {
        [Column("name"), MaxLength(100), Required]
        public string Name { get; set; }

        [Column("report_type")]
        public ReportType ReportType { get; set; }

        [Column("report_feed")]
        public ReportFeed ReportFeed { get; set; }

        [Column("sql_function")]
        public string SqlFunction { get; set; }

        [Column("module_id"), Required, ForeignKey("Module")]//, Index]
        public int ModuleId { get; set; }

        [Column("user_id"), ForeignKey("User")]//, Index]
        public int? UserId { get; set; }

        [Column("category_id"), ForeignKey("Category")]//, Index]
        public int? CategoryId { get; set; }

        [Column("group_field")]
        public string GroupField { get; set; }

        [Column("sort_field")]
        public string SortField { get; set; }

        [Column("sort_direction")]
        public SortDirection SortDirection { get; set; }

        [Column("sharing_type")]//, Index]
        public ReportSharingType SharingType { get; set; }

        [Column("filter_logic"), MaxLength(50)]
        public string FilterLogic { get; set; }

        public virtual Module Module { get; set; }

        public virtual TenantUser User { get; set; }

        public virtual ReportCategory Category { get; set; }

        public virtual ICollection<ReportField> Fields { get; set; }

        public virtual ICollection<ReportFilter> Filters { get; set; }

        public virtual ICollection<ReportAggregation> Aggregations { get; set; }

        public List<TenantUser> Shares { get; set; }
    }
}
