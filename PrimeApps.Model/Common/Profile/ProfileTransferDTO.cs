using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Profile
{
    [DataContract]
    public class ProfileTransferDTO
    {
        [DataMember]
        public int UserID { get; set; }
        [DataMember]
        public int TransferedProfileID { get; set; }
    }
}
