using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("template_permissions")]
    public class TemplatePermission : BaseEntity
    {
        [JsonIgnore]
        [Column("template_id"), ForeignKey("Template")]
        public int TemplateId { get; set; }

        [Column("profile_id"), ForeignKey("Profile"), Required]
        public int ProfileId { get; set; }

        [Column("type"), Required]
        public TemplatePermissionType Type { get; set; }

        public virtual Template Template { get; set; }

        public virtual Profile Profile { get; set; }
    }
}