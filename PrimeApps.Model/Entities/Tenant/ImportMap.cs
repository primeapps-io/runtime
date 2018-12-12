using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Tenant
{

    [Table("import_maps")]
    public class ImportMap : BaseEntity
    {
        [JsonIgnore]
        [Column("module_id"), ForeignKey("Module"), /*Index*/]
        public int ModuleId { get; set; }

        [Column("name"), Required, /*Index*/]
        public string Name { get; set; }

        [Column("skip")]
        public bool Skip { get; set; }

        [Column("mapping"), Required]
        public string Mapping { get; set; }

        public virtual Module Module { get; set; }
    }

}
