using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
    [Table("view_shares")]
    public class ViewShares
    {
        [Column("user_id"), ForeignKey("User")]
        public int UserId { get; set; }
        public virtual TenantUser User { get; set; }

        [Column("view_id"), ForeignKey("View")]
        public int ViewId { get; set; }
        public virtual View View { get; set; }
    }
}
