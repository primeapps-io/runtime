using Newtonsoft.Json;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Entities.Tenant
{
    [Table("functions")]

    public class Function : BaseEntity
    {
        [Column("name"), Required, StringLength(200)]
        public string Name { get; set; }

        [Column("dependencies")]
        public string Dependencies { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("handler"), Required]
        public string Handler { get; set; }

        [Column("runtime"), Required]
        public FunctionRuntime Runtime { get; set; }

        [Column("content_type"), Required]
        public FunctionContentType ContentType { get; set; }
    }
}
