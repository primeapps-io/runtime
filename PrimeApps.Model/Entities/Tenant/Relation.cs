using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("relations")]
    public class Relation : BaseEntity
    {
        [JsonIgnore]
        [Column("module_id"), ForeignKey("Module")]
        public int ModuleId { get; set; }

        [Column("related_module"), Required]
        public string RelatedModule { get; set; }

        [Column("relation_type"), Required]
        public RelationType RelationType { get; set; }

        [Column("relation_field"), MaxLength(50)]
        public string RelationField { get; set; }

        [Column("display_fields"), MaxLength(1000), Required]
        public string DisplayFields { get; set; }

        [Column("readonly")]
        public bool Readonly { get; set; }

        [Column("order")]
        public short Order { get; set; }

        [Column("label_en_singular"), MaxLength(50), Required]
        public string LabelEnSingular { get; set; }

        [Column("label_en_plural"), MaxLength(50), Required]
        public string LabelEnPlural { get; set; }

        [Column("label_tr_singular"), MaxLength(50), Required]
        public string LabelTrSingular { get; set; }

        [Column("label_tr_plural"), MaxLength(50), Required]
        public string LabelTrPlural { get; set; }

        [Column("detail_view_type")]
        public DetailViewType DetailViewType { get; set; }

        public virtual Module ParentModule { get; set; }

        [NotMapped]
        public virtual Module RelationModule { get; set; }

        [NotMapped]
        public string[] DisplayFieldsArray
        {
            get
            {
                if (string.IsNullOrWhiteSpace(DisplayFields))
                    return null;

                return DisplayFields.Split(',');
            }
            set
            {
                if (value != null && value.Length > 0)
                    DisplayFields = string.Join(",", value.Select(x => x.ToString()).ToArray());
            }
        }
    }
}
