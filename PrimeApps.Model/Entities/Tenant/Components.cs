using Newtonsoft.Json;
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
        [JsonProperty("name"), Column("name"), Required, MaxLength(15)]
        public string Name { get; set; }

        [JsonProperty("content"), Column("content")]
        public string Content { get; set; }

        [JsonProperty("type"), Column("type")]
        public ComponentType Type { get; set; }

        [JsonProperty("place"), Column("place")]
        public ComponentPlace Place { get; set; }

        [JsonProperty("module_id"), Column("module_id"), ForeignKey("Module")/*, Index*/]
        public int ModuleId { get; set; }

        [JsonProperty("order"), Column("order")]
        public int Order { get; set; }

        [JsonIgnore]
        public virtual Module Module { get; set; }
    }

	public enum ComponentType
	{
		NonSet = 0,
		Script = 1,
		Component = 2
	}

	public enum ComponentPlace
	{
		NonSet = 0,
		FieldChange = 1,
		BeforeCreate = 2,
		AfterCreate = 3,
		BeforeUpdate = 4,
		AfterUpdate = 5,
		BeforeDelete = 6,
		AfterDelete = 7,
        AfterRecordLoaded = 8,
		BeforeLookup = 9,
		PicklistFilter = 10,
        BeforeApproveProcess = 11,
        BeforeRejectProcess = 12,
        AfterApproveProcess = 13
    }
}
