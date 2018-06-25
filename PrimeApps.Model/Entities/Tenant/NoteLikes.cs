using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
	[Table("note_likes")]
	public class NoteLikes
	{
		[Column("user_id"), ForeignKey("User")]
		public int UserId { get; set; }
		public virtual TenantUser TenantUser { get; set; }

		[Column("note_id"), ForeignKey("Note")]
		public int NoteId { get; set; }
	    public virtual Note Note { get; set; }
	}
}
