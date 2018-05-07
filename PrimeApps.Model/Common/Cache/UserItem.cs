using System;

namespace PrimeApps.Model.Common.Cache
{
    /// <summary>
    /// This is the object which we store in the session for users.
    /// </summary>
    public class UserItem
    {

        /// <summary>
        /// Global Id for the user.
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// User name for the current session owner.
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// User name for the current session owner.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// Last activity date for the session.
        /// </summary>
        public DateTime LastActivity { get; set; }

        /// <summary>
        /// Gets or sets the culture.
        /// </summary>
        /// <value>The culture.</value>
        public string Culture { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>The currency.</value>
        public string Currency { get; set; }

		/// <summary>
		/// Gets or sets the currency.
		/// </summary>
		/// <value>The currency.</value>
		public string TimeZone { get; set; }

		/// <summary>
		/// Gets or sets the currency.
		/// </summary>
		/// <value>The currency.</value>
		public string Language { get; set; }

		/// <summary>
		/// Gets or sets the Tenant ID.
		/// </summary>
		/// <value>The Tenant ID.</value>
		public int TenantId { get; set; }


        /// <summary>
        /// This id is required for documents.
        /// </summary>
        /// <value>The Tenant Guid ID.</value>
        public Guid TenantGuid { get; set; }

        /// <summary>
        /// Gets or sets the active profile of user has admin rights.
        /// </summary>
        /// <value>The HasAdminProfile .</value>
        public bool HasAdminProfile { get; set; }

        /// <summary>
        /// Gets or sets the active role of user.
        /// </summary>
        /// <value>The Role.</value>
        public int Role { get; set; }

        /// <summary>
        /// Gets or sets the active profile id of user.
        /// </summary>
        /// <value>The Profile Id.</value>
        public int ProfileId { get; set; }

        /// <summary>
        /// Gets or sets the picklist language.
        /// </summary>
        /// <value>The Tenant Language.</value>
        public string TenantLanguage { get; set; }

        /// <summary>
        /// Gets or sets the App ID.
        /// </summary>
        /// <value>The AppID.</value>
        public int AppId { get; set; }

        /// <summary>
        /// Gets or sets the Warehouse Database Name.
        /// </summary>
        /// <value>The Warehouse Database Name.</value>
        public string WarehouseDatabaseName { get; set; }

    }
}