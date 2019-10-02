using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;
using System.Collections.Generic;
using System;
using System.Linq;

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
        public PublishStatusType Status { get; set; }

        [Column("content_type"), Required]
        public FunctionContentType ContentType { get; set; }

        [Column("environment"), MaxLength(10)]
        public string Environment { get; set; }

        public virtual ICollection<DeploymentFunction> Deployments { get; set; }

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
