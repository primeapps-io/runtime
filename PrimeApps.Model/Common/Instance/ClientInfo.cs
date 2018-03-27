using System;
using System.Collections.Generic;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Instance
{
    /// <summary>
    /// This is data transfer object for the comet. We use this object to return updates to the client.
    /// </summary>
    public class ClientInfo
    {
        /// <summary>
        /// constructor of the class
        /// </summary>
        public ClientInfo()
        {
            ComponentUpdates = new Dictionary<Component, DateTime?>();
        }

        /// <summary>
        ///Instance ID for the update
        /// </summary>
        public Guid InstanceID { get; set; }

        /// <summary>
        /// Components with update dates.
        /// </summary>
        public Dictionary<Component, DateTime?> ComponentUpdates { get; set; }
    }
}