using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("analytics")]
    public class Analytic : BaseEntity
    {
        [Column("label"), MaxLength(50), Required]
        public string Label { get; set; }

        [Column("powerbi_report_id")]
        public string PowerBiReportId { get; set; }

        [Column("pbix_url"), Required]
        public string PbixUrl { get; set; }

        [Column("sharing_type")]
        public AnalyticSharingType SharingType { get; set; }

        [Column("menu_icon"), MaxLength(100)]
        public string MenuIcon { get; set; }

        public List<AnalyticShares> Shares { get; set; }
    }
}
