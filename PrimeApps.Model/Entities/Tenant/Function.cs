using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;
using System.Collections.Generic;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("functions")]

    public class Function : BaseEntity
    {
        [Column("name"), Required, StringLength(100)]
        public string Name { get; set; }

        [Column("label"), StringLength(300)]
        public string Label { get; set; }

        [Column("dependencies")]
        public string Dependencies { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("handler"), Required]
        public string Handler { get; set; }

        [Column("runtime"), Required]
        public FunctionRuntime Runtime { get; set; }

        [JsonProperty("status"), Column("status")]
        public PublishStatus Status { get; set; }

        [Column("content_type"), Required]
        public FunctionContentType ContentType { get; set; }

        [Column("environment"), MaxLength(10)]
        public string Environment { get; set; }

        public virtual ICollection<DeploymentFunction> Deployments { get; set; }
    }
}
