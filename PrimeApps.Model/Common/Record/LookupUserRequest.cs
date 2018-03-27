namespace PrimeApps.Model.Common.Record
{
    public class LookupUserRequest
    {
        public int ModuleId { get; set; }
        public string SearchTerm { get; set; }
        public bool IsReadonly { get; set; }
    }
}
