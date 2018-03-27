using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Application
{
    [Table("template_permissions")]
    public class TemplatePermission : BaseEntity
    {
        [JsonIgnore]
        [Column("template_id"), ForeignKey("Template"),/* Index("template_permissions_IX_template_id_profile_id", 1, IsUnique = true)*/]
        public int TemplateId { get; set; }

        [Column("profile_id"), ForeignKey("Profile"), Required, /*Index("template_permissions_IX_template_id_profile_id", 2, IsUnique = true)*/]
        public int ProfileId { get; set; }

        [Column("type"), Required]
        public TemplatePermissionType Type { get; set; }

        public virtual Template Template { get; set; }

        public virtual Profile Profile { get; set; }
    }
}