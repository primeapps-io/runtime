using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("apps")]
    public class App : BaseEntity
    {
        [Column("name"), MaxLength(50)]
        public string Name { get; set; }

		[Column("label"), MaxLength(400)]
		public string Label { get; set; }

		[Column("description"), MaxLength(4000)]
        public string Description { get; set; }
		
        [Column("logo")]
        public string Logo { get; set; }

        [Column("template_id")]
        public int? TemplateId { get; set; }

		[Column("use_tenant_settings")]
		public bool UseTenantSettings { get; set; }

		//AppInfo One to One
		public virtual AppSetting Setting { get; set; }

		[JsonIgnore]
		public virtual ICollection<AppTemplate> Templates { get; set; }

		//Apps and Tenants One to Many 
		[JsonIgnore]
		public virtual ICollection<Tenant> Tenants { get; set; }

		[JsonIgnore]
		public virtual ICollection<TeamApp> AppTeams { get; set; }
	}
}
