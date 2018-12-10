using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Common.Role;

namespace PrimeApps.Model.Common.User
{
    public class ConsoleUser
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
    }
}
