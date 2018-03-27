namespace PrimeApps.Model.Common.Document
{
    public class DocumentFindRequest
    {
        public int ModuleId { get; set; }
        public int RecordId { get; set; }
        public int Limit { get; set; }
        public int Offset { get; set; }
    }
}
