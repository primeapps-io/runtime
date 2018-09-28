using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Tenant
{
	[Table("analytic_shares")]
	public class AnalyticShares
    {
	    [Column("user_id"), ForeignKey("User")]
		public int UserId { get; set; }
	    public virtual TenantUser TenantUser { get; set; }

	    [Column("analytic_id"), ForeignKey("Analytic")]
		public int AnaltyicId { get; set; }
	    public virtual Analytic Analytic { get; set; }
	}
}
