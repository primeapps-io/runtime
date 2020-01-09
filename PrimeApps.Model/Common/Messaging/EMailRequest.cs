using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Common.Messaging
{
    public class EmailRequest
    {
        public string[] ToAddresses { get; set; }
        public string Cc { get; set; }
        public string Bcc { get; set; }
        public string FromEmail { get; set; }
        public string FromName { get; set; }
        public int? ModuleId { get; set; }
        public int? RecordId { get; set; }
        public Template Template { get; set; }
    }
}
