using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.UserApps
{
    /// <summary>
    /// This DTO carries user account information to the client.
    /// </summary>
    [DataContract]
    public class UserAppInfo
    {

        public UserAppInfo()
        {
        }

        /// <summary>
        /// User DTO item.
        /// </summary>
        [DataMember]
        public int Id { get; set; }

        [DataMember]
        public int UserId { get; set; }

        [DataMember]
        public int TenantId { get; set; }

        [DataMember]
        public int MainTenantId { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public bool Active { get; set; }

        [DataMember]
        public int AppId { get; set; }
    }
}