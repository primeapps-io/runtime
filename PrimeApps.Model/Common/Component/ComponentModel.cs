using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.Component
{
    public class ComponentModel
    {
        [Column("id"), DataMember(Name = "id")]
        public int Id { get; set; }

        [JsonProperty("name"), DataMember(Name = "name"), Required, MaxLength(100)]
        public string Name { get; set; }

        [JsonProperty("content"), DataMember(Name = "content")]
        public string Content { get; set; }

        [JsonProperty("type"), DataMember(Name = "type"), Required]
        public ComponentType Type { get; set; }

        [JsonProperty("place"), DataMember(Name = "place")]
        public ComponentPlace Place { get; set; }

        [JsonProperty("module_id"), DataMember(Name = "module_id"), Required]
        public int ModuleId { get; set; }

        [JsonProperty("order"), DataMember(Name = "order")]
        public int Order { get; set; }

        [JsonProperty("label"), DataMember(Name = "label"), Required, MaxLength(100)]
        public string Label { get; set; }

        [JsonProperty("status"), DataMember(Name = "status")]
        public PublishStatus Status { get; set; }
    }
}
