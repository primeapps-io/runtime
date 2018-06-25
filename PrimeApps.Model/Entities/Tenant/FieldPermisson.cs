using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Application
{
    [Table("field_permissions")]
    public class FieldPermission : BaseEntity
    {
        [JsonIgnore]
        [Column("field_id"), ForeignKey("Field"), /*Index("field_permissions_IX_field_id_profile_id", 1, IsUnique = true)*/]
        public int FieldId { get; set; }

        [Column("profile_id"), ForeignKey("Profile"), Required,/* Index("field_permissions_IX_field_id_profile_id", 2, IsUnique = true)*/]
        public int ProfileId { get; set; }

        [Column("type"), Required]
        public FieldPermissionType Type { get; set; }

        public virtual Field Field { get; set; }

        public virtual Profile Profile { get; set; }
    }
}
