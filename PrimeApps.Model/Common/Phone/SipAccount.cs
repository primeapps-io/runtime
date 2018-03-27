namespace PrimeApps.Model.Common.Phone
{
    public class SipAccount
    {
        public string Connector { get; set; }
        public string Extension { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }

        public string CompanyKey { get; set; }
        public string Host { get; set; }

        public string CallerID { get; set; }
        public int Port { get; set; }

    }
}
