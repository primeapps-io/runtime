﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("user_settings")]
	public class PlatformUserSetting
    {
		[Column("user_id"), Key]
		public int UserId { get; set; }

		[Column("phone")]
		public string Phone { get; set; }

		[Column("culture")]
		public string Culture { get; set; }

		[Column("currency")]
		public string Currency { get; set; }

		[Column("time_zone")]
		public string TimeZone { get; set; }

		[Column("language")]
		public string Language { get; set; }

		public virtual PlatformUser User { get; set; }
	}
}
