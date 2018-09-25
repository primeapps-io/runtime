﻿using Newtonsoft.Json;
using PrimeApps.Model.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PrimeApps.Model.Entities.Platform
{
    [Table("tenants")]
    public class Tenant : BaseEntity
    {
        /// <summary>
        /// AppId
        /// </summary>
        [Column("app_id")]
        public int AppId { get; set; }

        [Column("guid_id")]//]//, Index]
        public Guid GuidId { get; set; }

        /// <summary>
        /// Instance Title
        /// </summary>
        [Column("title")]
        public string Title { get; set; }

        /// <summary>
        /// Owner of the instance.
        /// </summary>
        [Column("owner_id")]//]//, Index]
        public int OwnerId { get; set; }
		
		[Column("use_user_settings")]
		public bool UseUserSettings { get; set; }


		//Apps and Tenants One to Many 
		//[JsonIgnore]
		public virtual App App { get; set; }

        public virtual PlatformUser Owner { get; set; }
        //[JsonIgnore]
        public virtual ICollection<UserTenant> TenantUsers { get; set; }

        public virtual TenantSetting Setting { get; set; }

        public virtual TenantLicense License { get; set; }

    }
}
