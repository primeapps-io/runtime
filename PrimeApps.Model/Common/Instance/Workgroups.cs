using System.Collections.Generic;
using System.Runtime.Serialization;
using PrimeApps.Model.Common.User;

namespace PrimeApps.Model.Common.Instance
{
    /// <summary>
    /// This DTO carries required workgroup data's to the client.
    /// </summary>
    [DataContract]
    public class Workgroup
    {
        /// <summary>
        /// Constructor of the class.
        /// </summary>
        public Workgroup()
        {
            Users = new List<UserList>();
        }

        /// <summary>
        /// Instance/Workgroup ID
        /// </summary>
        [DataMember]
        public int TenantId { get; set; }

        /// <summary>
        /// Instance Title
        /// </summary>
        [DataMember]
        public string Title { get; set; }

        /// <summary>
        /// Administrator user of instance
        /// </summary>
        [DataMember]
        public int OwnerId { get; set; }

        /// <summary>
        /// All users of instance.
        /// </summary>
        [DataMember]
        public IList<UserList> Users { get; set; }
    }
}