using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Console.Constants
{
    public static class ApiResponseMessages
    {
        /// <summary>
        /// We use this constant to calculate the storage cost of a single entry on Activity feed.
        /// This called "Other Storage Size" in the system. It is a hypotetical storage cost and has no relation with Windows Azure Storage.
        /// </summary>
        public const string PERMISSION = "You dont have permission for this operation.";
        public const string ORGANIZATION_NOT_FOUND = "Organization not found.";
        public const string ORGANIZATION_REQUIRED = "Organization id required.";
        public const string OWN_ORGANIZATION = "You can not leave your own organization.";
    }
}
