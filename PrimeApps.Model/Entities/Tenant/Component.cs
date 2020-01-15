using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System;
using System.Linq;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("components")]
    public class Component : BaseEntity
    {
        [JsonProperty("name"), Column("name"), Required, MaxLength(100)]
        public string Name { get; set; }

        [JsonProperty("content"), Column("content")]
        public string Content { get; set; }

        [JsonProperty("type"), Column("type"), Required]
        public ComponentType Type { get; set; }

        [JsonProperty("place"), Column("place")]
        public ComponentPlace Place { get; set; }

        [JsonProperty("module_id"), Column("module_id"), ForeignKey("Module")]
        public int? ModuleId { get; set; }

        [JsonProperty("order"), Column("order")]
        public int Order { get; set; }

        [JsonProperty("status"), Column("status")]
        public PublishStatusType Status { get; set; }

        [JsonProperty("label"), Column("label"), Required, MaxLength(100)]
        public string Label { get; set; }

        [JsonProperty("environment"), Column("environment"), MaxLength(10)]
        public string Environment { get; set; }

        [JsonProperty("custom_url"), Column("custom_url")]
        public string CustomUrl { get; set; }

        [JsonIgnore]
        public virtual Module Module { get; set; }

        public virtual ICollection<DeploymentComponent> Deployments { get; set; }

        [NotMapped]
        public ICollection<EnvironmentType> EnvironmentList
        {
            get
            {
                if (string.IsNullOrEmpty(Environment))
                    return null;

                var list = Environment.Split(",");
                var data = new List<EnvironmentType>();

                foreach (var item in list)
                {
                    var value = (EnvironmentType)Enum.Parse(typeof(EnvironmentType), item);
                    data.Add(value);
                }

                return data;
            }

            set
            {
                Environment = string.Join(",", value.Select(x => (int)x));
            }
        }
    }
}
