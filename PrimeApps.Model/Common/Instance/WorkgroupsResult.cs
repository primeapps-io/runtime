using System.Collections.Generic;

namespace PrimeApps.Model.Common.Instance
{
    /// <summary>
    /// This dto is required by the instances view on the client side. We transfer formatted data about workgroups to the clients by the help of it.
    /// </summary>
    public class WorkgroupsResult
    {
        /// <summary>
        /// constructor of the class
        /// </summary>
        public WorkgroupsResult()
        {
            Personal = new List<Workgroup>();
            Shared = new List<Workgroup>();
            Invited = new List<Workgroup>();
        }

        /// <summary>
        /// Personal workgroups(owned workgroups)
        /// </summary>
        public IList<Workgroup> Personal { get; set; }
        
        /// <summary>
        /// Participated workgroups.
        /// </summary>
        public IList<Workgroup> Shared { get; set; }

        /// <summary>
        /// Workgroups that user has an invitation from.
        /// </summary>
        public IList<Workgroup> Invited { get; set; }
    }
}
