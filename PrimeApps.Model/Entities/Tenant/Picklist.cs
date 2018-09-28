using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("picklists")]
    public class Picklist : BaseEntity
    {
        [Column("system_type"), Required]
        public SystemType SystemType { get; set; }

        [Column("label_en"), MaxLength(50), Required]
        public string LabelEn { get; set; }

        [Column("label_tr"), MaxLength(50), Required]
        public string LabelTr { get; set; }

        [Column("migration_id")]
        public string MigrationId { get; set; }

        public virtual ICollection<PicklistItem> Items { get; set; }
    }
}
