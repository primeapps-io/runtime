using System.Collections.Generic;
using System.Runtime.Serialization;
using PrimeApps.Model.Common.Instance;
using PrimeApps.Model.Common.UserApps;

namespace PrimeApps.Model.Common.User
{
    /// <summary>
    /// This DTO carries user account information to the client.
    /// </summary>
    [DataContract]
    public class AccountInfo
    {

        public AccountInfo()
        {
            instances = new List<TenantInfo>();
        }

        /// <summary>
        /// User DTO item.
        /// </summary>
        [DataMember]
        public UserInfo user { get; set; }

        /// <summary>
        /// Instances list belongs to the user.
        /// </summary>
        [DataMember]
        public IList<TenantInfo> instances { get; set; }

        [DataMember]
        public string imageUrl { get; set; }

        [DataMember]
        public List<UserAppInfo> apps { get; set; } 
    }
}