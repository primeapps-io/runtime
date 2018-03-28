using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
    public class AnalyticTenantUser
    {
	    public int AnaltyicId { get; set; }
	    public Analytic Analytic { get; set; }

	    public int TenantUserId { get; set; }
	    public TenantUser TenantUser { get; set; }
	}
}
