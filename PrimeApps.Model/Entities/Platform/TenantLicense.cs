using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
	[Table("tenant_licenses")]
	public class TenantLicense
    {
		[Column("tenant_id")]
		public int TenantId { get; set; }

		[Column("user_license_count")]
		public int UserLicenseCount { get; set; }

		[Column("module_license_count")]
		public int ModuleLicenseCount { get; set; }

		[Column("analytics_license_count")]
		public int AnalyticsLicenseCount { get; set; }

		[Column("is_paid_customer")]
		public bool IsPaidCustomer { get; set; }

		[Column("is_deactivated")]
		public bool IsDeactivated { get; set; }

		[Column("is_suspended")]
		public bool IsSuspended { get; set; }

		[Column("deactivated_at")]
		public DateTime? DeactivatedAt { get; set; }

		[Column("suspended_at")]
		public DateTime? SuspendedAt { get; set; }

		//Tenant One to One
		public virtual Tenant Tenant { get; set; }
	}
}
