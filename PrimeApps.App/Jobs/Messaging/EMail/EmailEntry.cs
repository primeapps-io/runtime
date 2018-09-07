using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PrimeApps.App.Jobs.Messaging
{
    public class EmailEntry
    {
        /// <summary>
        /// Receiver Address
        /// </summary>
        public IList<string> EmailTo { get; set; }

        /// <summary>
        /// Sender Addres
        /// </summary>
        public string EmailFrom { get; set; }

        /// <summary>
        /// CC
        /// </summary>
        public string CC { get; set; }

        /// <summary>
        /// BCC
        /// </summary>
        public string Bcc { get; set; }


        /// <summary>
        /// Sender Name
        /// </summary>
        public string FromName { get; set; }

        /// <summary>
        /// Reply Address
        /// </summary>
        public string ReplyTo { get; set; }

        /// <summary>
        /// Subject of Email
        /// </summary>
        public string Subject { get; set; }

        /// <summary>
        /// Body of Email(usually formatted with templates)
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Unique ID for the email process, if it's in process by any worker role, it will be assigned by a unique id by that role.
        /// </summary>
        public Guid? UniqueID { get; set; }

        /// <summary>
        /// Creation date of Email.
        /// </summary>
        public DateTime? QueueTime { get; set; }

        /// <summary>
        /// Specify this property to send the email on a specific date.
        /// </summary>
        public DateTime? SendOn { get; set; }
    }
}