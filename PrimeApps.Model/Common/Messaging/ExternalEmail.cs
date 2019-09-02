namespace PrimeApps.Model.Common.Messaging
{
    public class ExternalEmail
    {
        public string Subject { get; set; }
        public string TemplateWithBody { get; set; }
        public string[] ToAddresses { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public int? RecordId { get; set; }
    }
}
