using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
    [Table("users_user_groups")]
    public class UsersUserGroup
    {
        [Column("user_id"), ForeignKey("User")]
        public int UserId { get; set; }
        public virtual TenantUser User { get; set; }

        [Column("group_id"), ForeignKey("UserGroup")]
        public int UserGroupId { get; set; }
        public virtual UserGroup UserGroup { get; set; }
    }
}
