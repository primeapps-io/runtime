using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Profile
{
    [DataContract]
    public class ProfilePermissionLightDTO
    {
        [DataMember]
        public int? ModuleId { get; set; }
        [DataMember]
        public int Type { get; set; }
        [DataMember]
        public bool Read { get; set; }
        [DataMember]
        public bool Write { get; set; }
        [DataMember]
        public bool Modify { get; set; }
        [DataMember]
        public bool Remove { get; set; }
    }
}
