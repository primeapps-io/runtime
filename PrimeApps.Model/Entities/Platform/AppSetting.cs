using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("app_settings")]
	public class AppSetting
	{
		[JsonIgnore]
		[Column("app_id"), Key]
		public int AppId { get; set; }

		/// <summary>
		/// Currency
		/// </summary>
		[Column("title")]
		public string Title { get; set; }


		/// <summary>
		/// Language
		/// </summary>
		[Column("description")]//]//, Index]
		public string Description { get; set; }

		/// <summary>
		/// Has Logo
		/// </summary>
		[Column("favicon")]
		public string Favicon { get; set; }

		/// <summary>
		/// Mail Sender Name
		/// </summary>
		[Column("color")]
		public string Color { get; set; }

		/// <summary>
		/// Mail Sender Email
		/// </summary>
		[Column("image")]//]//, Index]
		public string Image { get; set; }

		/// <summary>
		/// Custom Domain
		/// </summary>
		[Column("domain")]//]//, Index]
		public string Domain { get; set; }

		/// <summary>
		/// Custom Title
		/// </summary>
		[Column("mail_sender_name")]//]//, Index]
		public string MailSenderName { get; set; }

		/// <summary>
		/// Custom Login Title
		/// </summary>
		[Column("mail_sender_email")]
		public string MailSenderEmail { get; set; }

		[Column("currency")]
		public string Currency { get; set; }

		[Column("culture")]
		public string Culture { get; set; }

		[Column("time_zone")]
		public string TimeZone { get; set; }
		/// <summary>
		/// Language
		/// </summary>	
		[Column("language")]//]//, Index]
		public string Language { get; set; }

		//App One to One
		public virtual App App { get; set; }
	}
}
