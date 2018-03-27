using System.Runtime.Serialization;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Messaging
{
    [DataContract]
    public class MessageDTO
    {
        public MessageDTO()
        {
            /// Defined access level as system by default in order to manage conflicts with old queue items.
            AccessLevel = AccessLevelEnum.System;
        }
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Rev { get; set; }
        [DataMember]
        public int TenantId { get; set; }
        [DataMember]
        public MessageTypeEnum Type { get; set; }
        [DataMember]
        public AccessLevelEnum AccessLevel { get; set; }
    }

}
