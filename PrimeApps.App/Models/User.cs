using System;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;
//using DataAnnotationsExtensions;

namespace PrimeApps.App.Models
{
    /// <summary>
    /// This data transfer object carries user objects between clients and servers.
    /// </summary>
    [DataContract]
    public class UserDTO
    {
        /// <summary>
        /// User ID
        /// </summary>
        [DataMember]
        public Guid id { get; set; }

        /// <summary>
        /// Email address
        /// </summary>
        [DataMember]
        //[Email(ErrorMessage = "invalid")]
        [EmailAddress(ErrorMessage = "invalid")]
        [StringLength(50, ErrorMessage = "LengthExceed")]
        public string email { get; set; }

        /// <summary>
        /// First Name
        /// </summary>
        [DataMember]
        [StringLength(20, ErrorMessage = "LengthExceed")]
        public string firstName { get; set; }

        /// <summary>
        /// Last Name
        /// </summary>
        [DataMember]
        [StringLength(20, ErrorMessage = "LengthExceed")]
        public string lastName { get; set; }

        /// <summary>
        /// Phone Number
        /// </summary>
        [DataMember]
        [StringLength(20, ErrorMessage = "LengthExceed")]
        public string phone { get; set; }

        /// <summary>
        /// Password (Encrypted with MD5)
        /// </summary>
        [DataMember]
        public string password { get; set; }

        /// <summary>
        /// Has new user message?
        /// </summary>
        [DataMember]
        public  byte newUserMessage { get; set; }

        /// <summary>
        /// Avatar ID from file storage
        /// </summary>
        [DataMember]
        public string picture { get; set; }

        /// <summary>
        /// Default instance id for user(usually the first instance)
        /// </summary>
        [DataMember]
        public  Guid defaultInstanceID { get; set; }

        /// <summary>
        /// Last active instance id for the user, we use this to redirect users to last instance they have been, after user logins to client or refreshes it.
        /// </summary>
        [DataMember]
        public  Guid lastInstanceID { get; set; }

        /// <summary>
        /// Culture /language information for user.
        /// </summary>
        [DataMember]
        public  string culture { get; set; }

        /// <summary>
        /// Gets or sets the currency.
        /// </summary>
        /// <value>The currency.</value>
        [DataMember]
        public  string currency { get; set; }

        /// <summary>
        /// Task notification setting. This helps users to remember tasks that they have soon or belated deadlines. Worker roles sends notification emails about it every day.
        /// </summary>
        [DataMember]
        public  bool isTaskNotificationsEnabled { get; set; }

        /// <summary>
        /// Activity notification setting. When an activity which has a relation with this user has a new comment, this setting will help them to get a notification email about it.
        /// </summary>
        [DataMember]
        public  bool isActivityNotificationsEnabled { get; set; }

        /// <summary>
        /// Task completed notification setting. When a task created by the user is completed, this helps user to get a notification email about it.
        /// </summary>
        [DataMember]
        public bool isTaskCompletedNotificationsEnabled { get; set; }

        /// <summary>
        /// Task assigned notification setting. When a task assigned to user then user will get a notification email about it.
        /// </summary>
        [DataMember]
        public bool isTaskAssignedNotificationsEnabled { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether for this user is new note notifications enabled.
        /// </summary>
        /// <value><c>true</c> if for this user is new note notifications enabled; otherwise, <c>false</c>.</value>
        [DataMember]
        public bool isNewNoteNotificationsEnabled { get; set; }
    }
}