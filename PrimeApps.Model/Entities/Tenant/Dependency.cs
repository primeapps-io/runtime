using PrimeApps.Model.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Newtonsoft.Json;

namespace PrimeApps.Model.Entities.Tenant
{
	[Table("dependencies")]
	public class Dependency : BaseEntity
	{
		[JsonIgnore]
		[Column("module_id"), ForeignKey("Module")]
		public int ModuleId { get; set; }

		[Column("dependency_type"), Required]
		public DependencyType DependencyType { get; set; }

		[Column("parent_field"), MaxLength(50), Required]
		public string ParentField { get; set; }

		[Column("child_field"), MaxLength(50)]
		public string ChildField { get; set; }

		[Column("child_section"), MaxLength(50)]
		public string ChildSection { get; set; }

		[Column("values"), MaxLength(4000)]
		public string Values { get; set; }

		[Column("field_map_parent"), MaxLength(50)]
		public string FieldMapParent { get; set; }

		[Column("field_map_child"), MaxLength(50)]
		public string FieldMapChild { get; set; }

		[Column("value_map"), MaxLength(4000)]
		public string ValueMap { get; set; }

		[Column("otherwise")]
		public bool Otherwise { get; set; }

		[Column("clear")]
		public bool Clear { get; set; }

		public virtual Module Module { get; set; }

		[NotMapped]
		public string[] ValuesArray
		{
			get
			{
				if (string.IsNullOrWhiteSpace(Values))
					return null;

				return Values.Split(',');
			}
			set
			{
				if (value != null && value.Length > 0)
					Values = string.Join(",", value.Select(x => x.ToString()).ToArray());
			}
		}
	}
}
