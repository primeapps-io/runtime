using PrimeApps.Model.Common.User;
using PrimeApps.Model.Entities.Platform;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Instance
{
    /// <summary>
    /// This dto object transfers all required instance data to the client side.
    /// </summary>
    public class TenantInfo
    {
        public TenantInfo()
        {
        }
        private string _logoUrl;
        /// <summary>
        /// Tenant Name/Title
        /// </summary>
        [DataMember]
        public string title { get; set; }

        /// <summary>
        /// Tenant Currency
        /// </summary>
        [DataMember]
        public string currency { get; set; }

        /// <summary>
        /// Tenant Language
        /// </summary>
        [DataMember]
        public string language { get; set; }

        [DataMember]
        public TenantLicense licenses { get; set; }

        [DataMember]
        public virtual TenantSetting setting { get; set; }

        /// <summary>
        /// Logo Url
        /// </summary>
        [DataMember]
        public string logoUrl { get; set; }

        /// <summary>
        /// Has Sample Data
        /// </summary>
        [DataMember]
        public bool? hasSampleData { get; set; }

        /// <summary>
        /// Has Business Intelligence
        /// </summary>
        [DataMember]
        public bool? hasAnalytics { get; set; }

        /// <summary>
        /// Has Ofisim Phone
        /// </summary>
        [DataMember]
        public bool? hasPhone { get; set; }

        /// <summary>
        /// Tenant ID
        /// </summary>
        [DataMember]
        [Required(ErrorMessage = "required")]
        public int tenantId { get; set; }

        /// <summary>
        /// Administrator of the workgroup
        /// </summary>
        /// <value>The user identifier.</value>
        [DataMember]
        [Required(ErrorMessage = "required")]
        public int owner { get; set; }

        /// <summary>
        /// All users of instance.
        /// </summary>
        [DataMember]
        public IList<UserList> users { get; set; }
    }
}