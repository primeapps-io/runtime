using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using PrimeApps.App.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.AspNetCore.Mvc;

namespace PrimeApps.App.Results
{
    /// <summary>
    /// Helps downloading big files stored in Azure Blob Storage by streaming them chunkedly.
    /// </summary>
    public class FileDownloadResult : IActionResult
    {

        /// <summary>
        /// Public name of the file to be downloaded by client.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Blob containing the file to be downloaded by client.
        /// </summary>
        public CloudBlockBlob Blob { get; set; }

        public async Task<HttpResponseMessage> ExecuteAsync(CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage();
            
            response.Content =  new PushStreamContent(async (outputStream, _, __) =>
            {
                try
                {
                    await Blob.DownloadToStreamAsync(outputStream, AccessCondition.GenerateEmptyCondition(), new BlobRequestOptions()
                    {
                        ServerTimeout = TimeSpan.FromDays(1),
                        RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(10), 3)
                    }, null);
                }
                finally
                {
                    outputStream.Close();
                }
            });

            await Blob.FetchAttributesAsync();

            response.StatusCode = HttpStatusCode.OK;
            response.Content.Headers.ContentType = new MediaTypeHeaderValue(Blob.Properties.ContentType);
            response.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment") {
                 FileName=PublicName,
                 Size=Blob.Properties.Length
            };
            response.Content.Headers.ContentLength = Blob.Properties.Length;
            return response;
        }
    }
}