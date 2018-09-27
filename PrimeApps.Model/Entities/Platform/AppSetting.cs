using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("app_settings")]
	public class AppSetting
	{
		[JsonIgnore]
		[Column("app_id"), Key]
		public int AppId { get; set; }

		[Column("title")]
		public string Title { get; set; }

		[Column("description")]
		public string Description { get; set; }

		[Column("favicon")]
		public string Favicon { get; set; }

		[Column("color")]
		public string Color { get; set; }

		[Column("image")]
		public string Image { get; set; }

		[Column("domain")]
		public string Domain { get; set; }

		[Column("auth_domain")]
		public string AuthDomain { get; set; }

		[Column("mail_sender_name")]
		public string MailSenderName { get; set; }

		[Column("mail_sender_email")]
		public string MailSenderEmail { get; set; }

		[Column("currency")]
		public string Currency { get; set; }

		[Column("culture")]
		public string Culture { get; set; }

		[Column("time_zone")]
		public string TimeZone { get; set; }
		
		[Column("language")]
		public string Language { get; set; }

		[Column("banner", TypeName = "jsonb")]
		public string Banner { get; set; }

		[Column("google_analytics_code")]
		public string GoogleAnalyticsCode { get; set; }

		[Column("tenant_create_webhook")]
	    public string TenantCreateWebhook { get; set; }

	    public virtual App App { get; set; }
	}
}
