using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("apps")]
    public class App
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Column("name"), MaxLength(400)]
        public string Name { get; set; }

        [Column("description"), MaxLength(4000)]
        public string Description { get; set; }

        [Column("owner")]
        public int Owner { get; set; }

        [Column("logo")]
        public string Logo { get; set; }

        [Column("template_id")]
        public int? TemplateId { get; set; }

        [Column("deleted")]
        public bool Deleted { get; set; }

		//AppInfo One to One
		public virtual AppInfo Info { get; set; }

		//Apps and Tenants One to Many 
		[JsonIgnore]
		public virtual ICollection<Tenant> Tenants { get; set; }

		[JsonIgnore]
		public virtual ICollection<TeamApps> Teams { get; set; }
	}
}
