using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Common.Role;

namespace PrimeApps.Model.Common.User
{
    public class UserInfo
    {
        /// <summary>
        /// User ID
        /// </summary>
        [DataMember]
        public int ID { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [DataMember]
        public string email { get; set; }

        /// <summary>
        /// First Name
        /// </summary>
        [DataMember]
        public string firstName { get; set; }

        /// <summary>
        /// Last Name
        /// </summary>
        [DataMember]
        public string lastName { get; set; }

        /// <summary>
        ///Full name of user
        /// </summary>
        [DataMember]
        public string fullName { get; set; }

        /// <summary>
        /// Phone Number
        /// </summary>
        [DataMember]
        public string phone { get; set; }


        /// <summary>
        /// Avatar ID from file storage
        /// </summary>
        [DataMember]
        public string picture { get; set; }

        /// <summary>
        /// Contains currency for the user.
        /// </summary>
        [DataMember]
        public string currency { get; set; }

        /// <summary>
        /// Tenant Language
        /// </summary>
        [DataMember]
        public string tenantLanguage { get; set; }
        
        /// <summary>
        ///Language
        /// </summary>
        [DataMember]
        public string Language { get; set; }


        /// <summary>
        /// User profiles
        /// </summary>
        [DataMember]
        public ProfileDTO profile { get; set; }

        /// <summary>
        /// User profiles
        /// </summary>
        [DataMember]
        public RoleInfo role { get; set; }

        /// <summary>
        /// Has Analytics License
        /// </summary>
        [DataMember]
        public bool hasAnalytics { get; set; }
        
        [DataMember]
        public DateTime createdAt { get; set; }

        [DataMember]
        public int userLicenseCount { get; set; }

        [DataMember]
        public int tenantId { get; set; }

        [DataMember]
        public int appId { get; set; }

        [DataMember]
        public virtual List<int> groups { get; set; }

        [DataMember]
        public int moduleLicenseCount { get; set; }
        
        [DataMember]
        public bool isPaidCustomer { get; set; }

        [DataMember]
        public bool deactivated { get; set; }
    }
}
