using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
    public class TenantUserGroup
    {
        [Column("user_id")]
        public int UserId { get; set; }
        public TenantUser User { get; set; }

        public int UserGroupId { get; set; }
        public UserGroup UserGroup { get; set; }
    }
}
