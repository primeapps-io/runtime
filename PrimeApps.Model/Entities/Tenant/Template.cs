using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;
using System.Collections.Generic;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("templates")]
    public class Template : BaseEntity
    {
        [Column("template_type")]
        public TemplateType TemplateType { get; set; }

        [Column("code")]
        public string Code { get; set; }

        [Column("name"), MaxLength(200)]
        public string Name { get; set; }

        [Column("subject"), MaxLength(200)]
        public string Subject { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("language"), Required]
        public LanguageType Language { get; set; }

        [Column("module")]
        public string Module { get; set; }

        [Column("active")]
        public bool Active { get; set; }

        [Column("sharing_type")]
        public TemplateSharingType SharingType { get; set; }

        public List<TemplateShares> Shares { get; set; }

        [Column("system_type")]
        public SystemType SystemType { get; set; }

        public virtual ICollection<TemplatePermission> Permissions { get; set; }
    }
}
