using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Messaging
{
    [DataContract]
    public class SMSSettings
    {
        [DataMember]
        [Required()]
        public string Provider { get; set; }
        [DataMember]
        [Required()]
        public string UserName { get; set; }
        [DataMember]
        [Required()]
        public string Password { get; set; }
        [DataMember]
        [Required()]
        public string Alias { get; set; }
    }
}
