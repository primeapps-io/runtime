using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("field_permissions")]
    public class FieldPermission : BaseEntity
    {
        [JsonIgnore]
        [Column("field_id"), ForeignKey("Field")]
        public int FieldId { get; set; }

        [Column("profile_id"), ForeignKey("Profile"), Required]
        public int ProfileId { get; set; }

        [Column("type"), Required]
        public FieldPermissionType Type { get; set; }

        public virtual Field Field { get; set; }

        public virtual Profile Profile { get; set; }
    }
}
