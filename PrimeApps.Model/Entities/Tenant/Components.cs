using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
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

        [JsonProperty("module_id"), Column("module_id"), ForeignKey("Module")]
        public int ModuleId { get; set; }

        [JsonProperty("order"), Column("order")]
        public int Order { get; set; }

        [JsonIgnore]
        public virtual Module Module { get; set; }
    }
}
