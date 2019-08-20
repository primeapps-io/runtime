using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("functions")]

    public class Function : BaseEntity
    {
        [Column("name"), Required, StringLength(100)]
        public string Name { get; set; }

        [Column("label"), Required, StringLength(300)]
        public string Label { get; set; }

        [Column("dependencies")]
        public string Dependencies { get; set; }

        [Column("content")]
        public string Content { get; set; }

        [Column("handler"), Required]
        public string Handler { get; set; }

        [Column("status")]
        public PublishStatusType Status { get; set; }

        [Column("runtime"), Required]
        public FunctionRuntime Runtime { get; set; }

        [Column("content_type"), Required]
        public FunctionContentType ContentType { get; set; }
    }
}
