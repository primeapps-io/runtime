using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
	[Table("report_shares")]
	public class ReportTenantUser
    {
	    [Column("report_id"), Key, Required]
		public int ReportId { get; set; }
	    public Report Report { get; set; }

		[Column("user_id"), Key, Required]
		public int TenantUserId { get; set; }
	    public TenantUser TenantUser { get; set; }
	}
}
