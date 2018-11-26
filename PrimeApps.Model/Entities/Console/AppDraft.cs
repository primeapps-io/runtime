using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Console
{
    [Table("apps")]
    public class AppDraft : BaseEntity
    {
        [Column("name"), MaxLength(50)]
        public string Name { get; set; }

        [Column("label"), MaxLength(400)]
        public string Label { get; set; }

        [Column("description"), MaxLength(4000)]
        public string Description { get; set; }

        [Column("logo")]
        public string Logo { get; set; }

        [Column("templet_id"), ForeignKey("Templet")]
        public int TempletId { get; set; }

        [Column("use_tenant_settings")]
        public bool UseTenantSettings { get; set; }

        public virtual Templet Templet { get; set; }

        public virtual AppDraftSetting Setting { get; set; }

        [JsonIgnore]
        public virtual ICollection<AppCollaborator> Collaborators { get; set; }
    }
}
