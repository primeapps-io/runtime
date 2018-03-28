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
	    [Column("user_id"), ForeignKey("User")]
	    public int UserId { get; set; }
	    public TenantUser TenantUser { get; set; }

		[Column("report_id"), ForeignKey("Report")]
		public int ReportId { get; set; }
	    public Report Report { get; set; }

	}
}
