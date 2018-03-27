using System.Collections.Generic;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Common.Role;

namespace PrimeApps.Model.Common.Cache
{
    /// <summary>
    /// This is a class to keep instances, users and component updates, and to put in a structure for the client.
    /// </summary>
    public class TenantItem
    {
        public TenantItem()
        {
            Profiles = new List<ProfileLightDTO>();
        }

        /// <summary>
        /// Administrator of instance
        /// </summary>
        public int OwnerId { get; set; }

        /// <summary>
        /// Has Business Intelligence
        /// </summary>
        public bool? HasAnalytics { get; set; }

        /// <summary>
        /// Language
        /// </summary>
        public string Language { get; set; }

        /// <summary>
        /// States if this user is paid customer. 
        /// </summary>
        public bool IsPaidCustomer { get; set; }

        /// <summary>
        /// Gets or sets the active profile of user has analytics license.
        /// </summary>
        public bool HasAnalyticsLicense { get; set; }

        /// <summary>
        /// User license count 
        /// </summary>
        public int UserLicenseCount { get; set; }

        /// <summary>
        /// Module license count
        /// </summary>
        public int ModuleLicenseCount { get; set; }

        /// <summary>
        /// Is the account deactivated?
        /// </summary>
        public bool IsDeactivated { get; set; }

        /// <summary>
        /// Is the account suspended?
        /// </summary>
        public bool IsSuspended { get; set; }

        /// <summary>
        /// Active profiles of user for each instance.
        /// </summary>
        public IEnumerable<ProfileLightDTO> Profiles { get; set; }

        /// <summary>
        /// Users of instance
        /// </summary>
        public int[] Users { get; set; }

        /// <summary>
        /// Roles of user for each instance.
        /// </summary>
        public IEnumerable<RoleInfo> Roles { get; set; }
    }
}