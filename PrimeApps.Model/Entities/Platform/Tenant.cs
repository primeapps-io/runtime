using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("tenants")]
    public class Tenant : BaseEntity
    {
        [Column("app_id")]
        public int AppId { get; set; }

        [Column("guid_id")]
        public Guid GuidId { get; set; }

        [Column("title")]
        public string Title { get; set; }

        [Column("owner_id")]
        public int OwnerId { get; set; }
		
		[Column("use_user_settings")]
		public bool UseUserSettings { get; set; }

		public virtual App App { get; set; }

        public virtual PlatformUser Owner { get; set; }

        public virtual ICollection<UserTenant> TenantUsers { get; set; }

        public virtual TenantSetting Setting { get; set; }

        public virtual TenantLicense License { get; set; }

    }
}
