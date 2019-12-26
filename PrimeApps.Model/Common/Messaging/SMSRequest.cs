using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Messaging
{
    [DataContract]
    public class SMSRequest
    {
        [DataMember]
        public int ModuleId { get; set; }
        [DataMember]
        public string[] Ids { get; set; }
        [DataMember]
        public string Query { get; set; }
        [DataMember]
        public bool IsAllSelected { get; set; }
        [DataMember]
        public string Message { get; set; }
        [DataMember]
        public int TemplateId { get; set; }
        [DataMember]
        public string PhoneField { get; set; }
    }
}
