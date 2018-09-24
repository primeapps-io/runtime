using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.App.Jobs.Messaging
{
    /// <summary>
    /// Standard sms message.
    /// </summary>
    public class Message
    {
        public Message()
        {
            Recipients = new List<string>();
        }
        public string Body { get; set; }
        public string Subject { get; set; }
        public IList<string> Recipients { get; set; }
        public string AttachmentLink { get; set; }
        public string AttachmentName { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
    }
}
