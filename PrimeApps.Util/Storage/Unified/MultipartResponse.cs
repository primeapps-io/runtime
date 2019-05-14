using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Util.Storage.Unified
{
    public class MultipartResponse
    {
        public string UploadId { get; set; }
        public string ETag { get; set; }
        public MultipartStatusEnum Status { get; set; }
        public string PublicURL {get;set;}
    }
}
