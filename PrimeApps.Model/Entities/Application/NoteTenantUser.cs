using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
    public class NoteTenantUser
    {
	    public int NoteId { get; set; }
	    public Note Note { get; set; }

	    public int TenantUserId { get; set; }
	    public TenantUser TenantUser { get; set; }
	}
}
