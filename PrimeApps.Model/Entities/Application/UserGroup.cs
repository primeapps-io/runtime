using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Application
{
    [Table("user_groups")]
    public class UserGroup : BaseEntity
    {
        [Column("name")]
        public string Name { get; set; }

        [Column("description")]
        public string Description { get; set; }
        
        public virtual ICollection<TenantUserGroup> Users { get; set; }
    }
}
