using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Common.Role;
using System;
using System.Collections.Generic;
using System.Text;

namespace PrimeApps.Model.Entities.Application
{
    public class InstanceItem
    {
        public InstanceItem()
        {
            Profiles = new List<ProfileLightDTO>();
        }

        /// <summary>
        /// Administrator of instance
        /// </summary>
        public Guid Admin { get; set; }

        /// <summary>
        /// Has Business Intelligence
        /// </summary>
        public bool? HasAnalytics { get; set; }

        /// <summary>
        /// Culture
        /// </summary>
        public string Culture { get; set; }

        /// <summary>
        /// Active profiles of user for each instance.
        /// </summary>
        public IEnumerable<ProfileLightDTO> Profiles { get; set; }

        /// <summary>
        /// Users of instance
        /// </summary>
        public Guid[] Users { get; set; }

        /// <summary>
        /// Roles of user for each instance.
        /// </summary>
        public IEnumerable<RoleInfo> Roles { get; set; }
    }
}
