using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Console
{
    [Table("app_profile_permissions")]
    public class AppProfilePermission
    {
        [Column("id"), Key, DatabaseGenerated(DatabaseGeneratedOption.Identity), Required]
        public int Id { get; set; }

        [Column("profile_id"), ForeignKey("Profile"), Required]
        public int ProfileId { get; set; }

        [Column("feature")]
        public PlatformFeature Feature { get; set; }

        [Column("read")]
        public bool Read { get; set; }

        [Column("write")]
        public bool Write { get; set; }

        [Column("modify")]
        public bool Modify { get; set; }

        [Column("remove")]
        public bool Remove { get; set; }
        
        public virtual AppProfile Profile { get; set; }
    }
}
