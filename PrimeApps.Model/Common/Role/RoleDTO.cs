using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Role
{
    [DataContract]
    public class RoleDTO
    {
        public RoleDTO()
        {
            _owners = new List<string>();
        }

        [DataMember]
        public int Id { get; set; }
        [DataMember]
        public string LabelEn { get; set; }
        [DataMember]
        public string LabelTr { get; set; }
        [DataMember]
        public string DescriptionEn { get; set; }
        [DataMember]
        public string DescriptionTr { get; set; }
        [DataMember]
        public bool Master { get; set; }
        public string OwnersRaw
        {
            get { return string.Join(",", _owners); }
            set { _owners = value?.Split(new char[] { ',' }).ToList() ?? new List<string>(); }
        }
        private List<string> _owners { get; set; }
        [DataMember]

        public List<string> Owners
        {
            get
            {
                return _owners;
            }
            set
            {
                _owners = value;
            }
        }
        [DataMember]
        public int? ReportsTo { get; set; }
        [DataMember]
        public int CreatedBy { get; set; }
        [DataMember]
        public IEnumerable<int> Users { get; set; }
        [DataMember]
        public bool ShareData { get; set; }
    }
}
