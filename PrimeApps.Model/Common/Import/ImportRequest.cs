namespace PrimeApps.Model.Common.Import
{
    public class ImportRequest
    {
        public int? ModuleId { get; set; }

        public int? UserId { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }
    }
}
