namespace PrimeApps.Model.Common.Note
{
    public class NoteRequest
    {
        public int? ModuleId { get; set; }

        public int? RecordId { get; set; }

        public int Limit { get; set; }

        public int Offset { get; set; }
    }
}
