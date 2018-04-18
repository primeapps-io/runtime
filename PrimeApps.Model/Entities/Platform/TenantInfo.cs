using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("tenant_info")]
	public class TenantInfo
    {
		[JsonIgnore]
		[Column("tenant_id"), Key]
		public int TenantId { get; set; }

		/// <summary>
		/// Currency
		/// </summary>
		[Column("currency")]
		public string Currency { get; set; }


		/// <summary>
		/// Language
		/// </summary>
		[Column("language")]//]//, Index]
		public string Language { get; set; }

		/// <summary>
		/// Has Logo
		/// </summary>
		[Column("logo")]
		public string Logo { get; set; }

		/// <summary>
		/// Mail Sender Name
		/// </summary>
		[Column("mail_sender_name")]
		public string MailSenderName { get; set; }

		/// <summary>
		/// Mail Sender Email
		/// </summary>
		[Column("mail_sender_email")]//]//, Index]
		public string MailSenderEmail { get; set; }

		/// <summary>
		/// Custom Domain
		/// </summary>
		[Column("custom_domain")]//]//, Index]
		public string CustomDomain { get; set; }

		/// <summary>
		/// Custom Title
		/// </summary>
		[Column("custom_title")]//]//, Index]
		public string CustomTitle { get; set; }

		/// <summary>
		/// Custom Login Title
		/// </summary>
		[Column("custom_description")]
		public string CustomDescription { get; set; }

		/// <summary>
		/// Custom Favicon
		/// </summary>
		[Column("custom_favicon")]
		public string CustomFavicon { get; set; }

		/// <summary>
		/// Custom Color
		/// </summary>
		[Column("custom_color")]
		public string CustomColor { get; set; }

		/// <summary>
		/// Custom Image
		/// </summary>
		[Column("custom_image")]
		public string CustomImage { get; set; }

		//Tenant One to One
		public virtual Tenant Tenant { get; set; }
	}
}
