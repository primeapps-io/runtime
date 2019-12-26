using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("charts")]
    public class Chart : BaseEntity
    {
        [Column("chart_type")]
        public ChartType ChartType { get; set; }

        [Column("caption_en"), MaxLength(100)]
        public string CaptionEn { get; set; }    
        [Column("caption_tr"), MaxLength(100)]
        public string CaptionTr { get; set; }

        [Column("sub_caption_en"), MaxLength(200)]
        public string SubCaptionEn { get; set; }    
        
        [Column("sub_caption_tr"), MaxLength(200)]
        public string SubCaptionTr { get; set; }

        [Column("theme")]
        public ChartTheme Theme { get; set; }

        [Column("x_axis_name_en"), MaxLength(80)]
        public string XaxisNameEn { get; set; }  
        
        [Column("x_axis_name_tr"), MaxLength(80)]
        public string XaxisNameTr { get; set; }
        

        [Column("y_axis_name_en"), MaxLength(80)]
        public string YaxisNameEn { get; set; }
        
        [Column("y_axis_name_tr"), MaxLength(80)]
        public string YaxisNameTr { get; set; }

        [Column("report_id"), ForeignKey("Report")]
        public int? ReportId { get; set; }

        public virtual Report Report { get; set; }
    }
}
