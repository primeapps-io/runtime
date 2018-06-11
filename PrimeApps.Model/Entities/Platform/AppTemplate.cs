using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("app_template")]
	public class AppTemplate
	{
		[JsonIgnore]
		[Column("app_id"), Key]
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

		[Column("system_code")]
		public string SystemCode { get; set; }

		[Column("active")]
		public bool Active { get; set; }

		[Column("mail_sender_name")]//]//, Index]
		public string MailSenderName { get; set; }

		/// <summary>
		/// Custom Login Title
		/// </summary>
		[Column("mail_sender_email")]
		public string MailSenderEmail { get; set; }

		//App One to One
		public virtual App App { get; set; }
	}

}
