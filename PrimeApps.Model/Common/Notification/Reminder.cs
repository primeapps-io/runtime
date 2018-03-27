using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Notification
{
    [DataContract]
    public class ReminderDTO
    {
        [DataMember]
        public string Id { get; set; }
        [DataMember]
        public string Rev { get; set; }
        [DataMember]
        public int TenantId { get; set; }
    }
}
