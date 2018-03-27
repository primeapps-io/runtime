using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static PrimeApps.Model.Enums.ActionButtonEnum;

namespace PrimeApps.Model.Entities.Application
{
	[Table("components")]

	public class Components : BaseEntity
	{
		[Column("name"), Required, MaxLength(15)]
		public string Name { get; set; }

		[Column("content")]
		public string Content { get; set; }

		[Column("type")]
		public ComponentType Type { get; set; }

		[Column("place")]
		public ComponentPlace Place { get; set; }

		[Column("module_id"), ForeignKey("Module")]//, Index]
		public int ModuleId { get; set; }

		[Column("order")]
		public int Order { get; set; }

		public virtual Module Module { get; set; }
	}

	public enum ComponentType
	{
		Script = 1
	}

	public enum ComponentPlace
	{
		FieldChange = 1,
		BeforeCreate = 2,
		AfterCreate = 3,
		BeforeUpdate = 4,
		AfterUpdate = 5,
		BeforeDelete = 6,
		AfterDelete = 7
	}
}
