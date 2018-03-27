using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrimeApps.App.Helpers
{
    /// <summary>
    /// This class is to store constants in webservice.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// We use this constant to calculate the storage cost of a single entry on Activity feed.
        /// This called "Other Storage Size" in the system. It is a hypotetical storage cost and has no relation with Windows Azure Storage.
        /// </summary>
        public const long SINGLE_ENTITY_SIZE = 104857;
        public const long ADDON_STORAGE_SIZE = 1073741824;
        public static string[] CULTURES = new string[] { "en-US", "tr-TR" };
        public static string[] CURRENCIES = new string[] { "USD", "TRY" };
    }
}