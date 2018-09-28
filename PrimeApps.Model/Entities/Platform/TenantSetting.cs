using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("tenant_settings")]
	public class TenantSetting
	{
		[JsonIgnore]
		[Column("tenant_id"), Key]
		public int TenantId { get; set; }

		[Column("currency")]
		public string Currency { get; set; }

		[Column("culture")]
		public string Culture { get; set; }

		[Column("time_zone")]
		public string TimeZone { get; set; }

		[Column("language")]
		public string Language { get; set; }
        
		[Column("logo")]
		public string Logo { get; set; }

		[Column("mail_sender_name")]
		public string MailSenderName { get; set; }

		[Column("mail_sender_email")]
		public string MailSenderEmail { get; set; }

		[Column("custom_domain")]
		public string CustomDomain { get; set; }

		[Column("custom_title")]
		public string CustomTitle { get; set; }

		[Column("custom_description")]
		public string CustomDescription { get; set; }

		[Column("custom_favicon")]
		public string CustomFavicon { get; set; }

		[Column("custom_color")]
		public string CustomColor { get; set; }

		[Column("custom_image")]
		public string CustomImage { get; set; }

		[Column("has_sample_data")]
		public bool HasSampleData { get; set; }

		public virtual Tenant Tenant { get; set; }
	}
}
