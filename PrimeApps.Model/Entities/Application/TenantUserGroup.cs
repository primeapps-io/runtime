using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
    class TenantUserGroup
    {
        public int UserId { get; set; }
        public TenantUser User { get; set; }


        public int UserGroupId { get; set; }
        public UserGroup UserGroup { get; set; }
    }
}
