using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Profile
{
    [DataContract]
    public class ProfileRemovalDTO
    {
        [DataMember]
        public ProfileDTO RemovedProfile { get; set; }

        [DataMember]
        public ProfileDTO TransferProfile { get; set; }
    }
}
