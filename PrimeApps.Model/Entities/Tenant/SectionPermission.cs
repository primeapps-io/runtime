using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Tenant
{

    [Table("section_permissions")]
    public class SectionPermission : BaseEntity
    {
        [JsonIgnore]
        [Column("section_id"), ForeignKey("Section")]
        public int SectionId { get; set; }

        [Column("profile_id"), ForeignKey("Profile"), Required]
        public int ProfileId { get; set; }

        [Column("type"), Required]
        public SectionPermissionType Type { get; set; }

        public virtual Section Section { get; set; }

        public virtual Profile Profile { get; set; }
    }
}
