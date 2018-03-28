using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
	[Table("analytic_shares")]
	public class AnalyticTenantUser
    {
	    [Column("user_id"), ForeignKey("User")]
		public int UserId { get; set; }
	    public TenantUser TenantUser { get; set; }

	    [Column("analytic_id"), ForeignKey("Analytic")]
		public int AnaltyicId { get; set; }
	    public Analytic Analytic { get; set; }
	}
}
