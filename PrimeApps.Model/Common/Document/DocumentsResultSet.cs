using System.Collections.Generic;
using System.Runtime.Serialization;

namespace PrimeApps.Model.Common.Document
{
    public class DocumentsResultSet
    {
        [DataMember]
        public IList<DocumentResult> Documents { get; set; }
        [DataMember]
        public int TotalDocumentsCount { get; set; }
        [DataMember]
        public int FilteredDocumentsCount { get; set; }
    }
}
