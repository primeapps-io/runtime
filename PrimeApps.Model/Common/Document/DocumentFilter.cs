using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Common.Document
{
    public class DocumentFilter
    {
        public string SearchText { get; set; }

        public DocumentFilterQueryOperator QueryOperator { get; set; }

        public DocumentFilterOperator Operator { get; set; }

    }
}
