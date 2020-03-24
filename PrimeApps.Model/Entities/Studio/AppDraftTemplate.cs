using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Studio
{
	[Table("app_templates")]
	public class AppDraftTemplate : BaseEntity
	{
		[JsonIgnore]
		[Column("app_id"), ForeignKey("App"), JsonProperty("app_id")]
		public int AppId { get; set; }

		[Column("name"), MaxLength(200)]
		public string Name { get; set; }

		[Column("subject"), MaxLength(200)]
		public string Subject { get; set; }

		[Column("content")]
		public string Content { get; set; }

		[Column("language")]
		public string Language { get; set; }

		[Column("type")]
		public AppTemplateType Type { get; set; }

		[Column("system_code"), JsonProperty("system_code")]
		public string SystemCode { get; set; }

		[Column("active")]
		public bool Active { get; set; }

		[Column("settings", TypeName = "jsonb")]
		public string Settings { get; set; }

		public virtual AppDraft App { get; set; }
	}
}
