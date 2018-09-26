using Newtonsoft.Json;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Entities.Tenant
{

    [Table("section_permissions")]
    public class SectionPermission : BaseEntity
    {
        [JsonIgnore]
        [Column("section_id"), ForeignKey("Section"), /*Index("section_permissions_IX_section_id_profile_id", 1, IsUnique = true)*/]
        public int SectionId { get; set; }

        [Column("profile_id"), ForeignKey("Profile"), Required, /*Index("section_permissions_IX_section_id_profile_id", 2, IsUnique = true)*/]
        public int ProfileId { get; set; }

        [Column("type"), Required]
        public SectionPermissionType Type { get; set; }

        public virtual Section Section { get; set; }

        public virtual Profile Profile { get; set; }
    }
}
