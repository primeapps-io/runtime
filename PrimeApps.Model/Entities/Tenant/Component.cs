using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;
using System.Collections.Generic;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("components")]
    public class Component : BaseEntity
    {
        [JsonProperty("name"), Column("name"), Required, MaxLength(15)]
        public string Name { get; set; }

        [JsonProperty("content"), Column("content")]
        public string Content { get; set; }

        [JsonProperty("type"), Column("type"), Required]
        public ComponentType Type { get; set; }

        [JsonProperty("place"), Column("place")]
        public ComponentPlace Place { get; set; }

        [JsonProperty("module_id"), Column("module_id"), ForeignKey("Module"), Required]
        public int ModuleId { get; set; }

        [JsonProperty("order"), Column("order")]
        public int Order { get; set; }

        [JsonProperty("status"), Column("status")]
        public PublishStatus Status { get; set; }

        [JsonIgnore]
        public virtual Module Module { get; set; }

        public virtual ICollection<DeploymentComponent> Deployments { get; set; }
    }
}
