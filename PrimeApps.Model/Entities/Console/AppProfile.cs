using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Console
{
    [Table("app_profiles")]
    public class AppProfile : BaseEntity
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }

        [Column("order")]
        public int Order { get; set; }

        [Column("system_code")]
        public string SystemCode { get; set; }

        [Column("app_id"), ForeignKey("AppDraft")]
        public int AppId { get; set; }

        public virtual IList<AppProfilePermission> Permissions { get; set; }

        public virtual AppDraft AppDraft { get; set; }
    }
}
