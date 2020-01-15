using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("components")]
    public class Component : BaseEntity
    {
        [JsonProperty("name"), Column("name"), Required, MaxLength(15)]
        public string Name { get; set; }

        [JsonProperty("content"), Column("content")]
        public string Content { get; set; }

        [JsonProperty("status"), Column("status")]
        public PublishStatusType Status { get; set; }

        [JsonProperty("type"), Column("type")]
        public ComponentType Type { get; set; }

        [JsonProperty("place"), Column("place")]
        public ComponentPlace Place { get; set; }

        [JsonProperty("module_id"), Column("module_id")]
        public int ModuleId { get; set; }

        [JsonProperty("order"), Column("order")]
        public int Order { get; set; }
    }
}
