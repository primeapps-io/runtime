namespace PrimeApps.Model.Common.Phone
{
    public class NewSipAccount
    {
        public string Connector { get; set; }
        public string Extension { get; set; }
        public string Password { get; set; }
        public int UserId { get; set; }
        public string CompanyKey { get; set; }
        public bool IsAutoRegister { get; set; }
        public bool IsAutoRecordDetail { get; set; }
        public string RecordDetailModuleName { get; set; }
        public string RecordDetailPhoneFieldName { get; set; }
    }
}
