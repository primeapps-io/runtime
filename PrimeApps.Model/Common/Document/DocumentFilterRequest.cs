using System.Collections.Generic;

namespace PrimeApps.Model.Common.Document
{
    public class DocumentFilterRequest
    {
        public string Module { get; set; }

        public List<DocumentFilter> Filters { get; set; }

        public int Top { get; set; }

        public int Skip { get; set; }

    }
}
