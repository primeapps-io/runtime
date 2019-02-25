using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Studio
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

        [Column("organization_id"), ForeignKey("Organization")]
        public int OrganizationId { get; set; }

        [Column("templet_id"), ForeignKey("Templet")]
        public int TempletId { get; set; }

        [Column("use_tenant_settings")]
        public bool UseTenantSettings { get; set; }

        [Column("status")]
        public PublishStatus Status { get; set; }
        
        public virtual Templet Templet { get; set; }
        
        public virtual AppDraftSetting Setting { get; set; }

        public virtual Organization Organization { get; set; }
        
        public virtual ICollection<AppCollaborator> Collaborators { get; set; }

        public virtual ICollection<Deployment> Deployments { get; set; }
    }
}
