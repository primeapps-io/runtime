using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
	[Table("template_shares")]
	public class TemplateTenantUser
    {
	    [Column("user_id"), ForeignKey("User")]
	    public int UserId { get; set; }
	    public TenantUser TenantUser { get; set; }

		[Column("template_id"), ForeignKey("Template")]
		public int TemplateId { get; set; }
	    public Template Template { get; set; }

	}
}
