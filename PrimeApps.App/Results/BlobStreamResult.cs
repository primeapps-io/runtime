using Microsoft.AspNetCore.Mvc;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace PrimeApps.App.Results
{
    public class BlobStreamResult : FileResult
    {
        private CloudBlockBlob _blob;
        public BlobStreamResult(CloudBlockBlob blob, string contentType) : base(contentType)
        {
            _blob = blob;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override void ExecuteResult(ActionContext context)
        {
            base.ExecuteResult(context);
        }

        public override Task ExecuteResultAsync(ActionContext context)
        {
            
            _blob.DownloadToStreamAsync(context.HttpContext.Response.Body);
            return base.ExecuteResultAsync(context);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return base.ToString();
        }
    }
}
