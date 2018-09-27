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
    [Table("settings")]
    public class Setting : BaseEntity
    {
        [Column("type"), Required]
        //[Index]
        public SettingType Type { get; set; }

        [Column("user_id"), ForeignKey("User")/*, Index("settings_IX_user_id")*/]
        public int? UserId { get; set; }

        [Column("key"), Required]
        //[Index]
        public string Key { get; set; }

        [Column("value"), Required]
        public string Value { get; set; }

        public virtual TenantUser User { get; set; }
    }
}
