namespace PrimeApps.Studio.Storage.Unified
{
    public class MultipartResponse
    {
        public string UploadId { get; set; }
        public string ETag { get; set; }
        public MultipartStatusEnum Status { get; set; }
        public string PublicURL { get; set; }

    }
}
