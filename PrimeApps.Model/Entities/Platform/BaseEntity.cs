using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
	public class BaseEntity
	{
		[Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
		public int Id { get; set; }

		[Column("created_by")]//, Index]
		public int CreatedById { get; set; }

		[Column("updated_by")]//, Index]
		public int? UpdatedById { get; set; }

		[Column("created_at"), Required]//, Index]
		public DateTime CreatedAt { get; set; }

		[Column("updated_at")]//, Index]
		public DateTime? UpdatedAt { get; set; }

		[Column("deleted")]//, Index]
		public bool Deleted { get; set; }

		[ForeignKey("CreatedBy")]
		public virtual PlatformUser CreatedBy { get; set; }

		[ForeignKey("UpdatedBy")]
		public virtual PlatformUser UpdatedBy { get; set; }
	}
}
