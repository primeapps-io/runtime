using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
    public class TemplateTenantUser
    {
	    public int TemplateId { get; set; }
	    public Template Template { get; set; }

	    public int TenantUserId { get; set; }
	    public TenantUser TenantUser { get; set; }
	}
}
