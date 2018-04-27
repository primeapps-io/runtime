namespace PrimeApps.Model.Common.User
{
    /// <summary>
    /// This data transfer is designed for user lists of workgroups. It carries only mandatory user fields.
    /// </summary>
    public class UserList
    {

        /// <summary>
        /// User ID
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Username
        /// </summary>
        public string userName { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        public string email { get; set; }

        /// <summary>
        /// Is this a real user or just invited?
        /// </summary>
        public bool hasAccount { get; set; }

        /// <summary>
        /// Is this user administrator of the workgroup?
        /// </summary>
        public bool isAdmin { get; set; }
    }
}