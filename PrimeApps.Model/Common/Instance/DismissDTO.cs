using System;
using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Instance
{
    /// <summary>
    /// This dto object is required by user dismiss process on the client side. We use this object to transfer data from clients to servers.
    /// </summary>
    [DataContract]
    public class DismissDTO
    {
        /// <summary>
        /// User Id to dismiss
        /// </summary>
        [DataMember]
        public Guid UserID { get; set; }

        /// <summary>
        /// Instance ID to dismiss
        /// </summary>
        [DataMember]
        public Guid InstanceID { get; set; }

        /// <summary>
        /// Email address to dismiss.
        /// </summary>
        [DataMember]
        public string EMail { get; set; }

        /// <summary>
        /// Is this user has an account?
        /// </summary>
        [DataMember]
        public bool HasAccount { get; set; }
    }
}