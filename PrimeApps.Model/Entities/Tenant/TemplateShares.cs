using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Tenant
{
	[Table("template_shares")]
	public class TemplateShares
    {
	    [Column("user_id"), ForeignKey("User")]
	    public int UserId { get; set; }
	    public virtual TenantUser TenantUser { get; set; }

		[Column("template_id"), ForeignKey("Template")]
		public int TemplateId { get; set; }
	    public virtual Template Template { get; set; }

	}
}
