using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace PrimeApps.Model.Common.App
{
    public class AppDraft
    {
        [DataMember]
        public int id { get; set; }

        [DataMember]
        public string name { get; set; }

        [DataMember]
        public string label { get; set; }

        [DataMember]
        public string description { get; set; }

        [DataMember]
        public int templetId { get; set; }
    }
}
