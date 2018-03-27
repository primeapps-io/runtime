namespace PrimeApps.Model.Common.Phone
{
    public class SipProvider
    {
        public string Provider { get; set; }
        public string Host { get; set; }
        public string CompanyKey { get; set; }
        public string Port { get; set; }
        public bool IsWebSocket { get; set; }
        public string WebSocketPort { get; set; }
        public string ProviderDomain { get; set; }


    }
}
