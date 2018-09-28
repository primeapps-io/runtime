using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("picklist_items")]
    public class PicklistItem : BaseEntity
    {
        [JsonIgnore]
        [Column("picklist_id"), ForeignKey("Picklist")]
        public int PicklistId { get; set; }

        [Column("label_en"), MaxLength(100), Required]
        public string LabelEn { get; set; }

        [Column("label_tr"), MaxLength(100), Required]
        public string LabelTr { get; set; }

        [Column("value"), MaxLength(100)]
        public string Value { get; set; }

        [Column("value2"), MaxLength(100)]
        public string Value2 { get; set; }

        [Column("value3"), MaxLength(100)]
        public string Value3 { get; set; }

        [Column("system_code"), MaxLength(50)]
        public string SystemCode { get; set; }

        [Column("order")]
        public short Order { get; set; }

        [Column("inactive")]
        public bool Inactive { get; set; }

        [Column("migration_id"), MaxLength(50)]
        public string MigrationId { get; set; }

        public virtual Picklist Picklist { get; set; }
    }
}
