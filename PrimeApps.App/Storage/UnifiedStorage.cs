using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace PrimeApps.App.Storage
{
    /// <summary>
    /// Unified Storage library based on Amazon S3
    /// </summary>
    public class UnifiedStorage
    {
        private IAmazonS3 _client;

        public UnifiedStorage(IAmazonS3 client)
        {
            _client = client;
        }
        /// <summary>
        /// Uploads a file stream into a bucket.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task Upload(string bucket, string key, Stream stream)
        {
            await CreateBucketIfNotExists(bucket);

            using (TransferUtility transUtil = new TransferUtility(_client))
            {
                await transUtil.UploadAsync(stream, bucket, key);
            }
        }

        /// <summary>
        /// Downloads files from S3 as FileStreamResult(Chunked)
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public async Task<FileStreamResult> Download(string bucket, string key, string fileName)
        {
            GetObjectResponse file = await _client.GetObjectAsync(bucket, key);

            FileStreamResult result = new FileStreamResult(file.ResponseStream, file.Headers.ContentType)
            {
                FileDownloadName = fileName,
                LastModified = file.LastModified,
                EntityTag = new EntityTagHeaderValue(file.ETag)
            };
            return result;
        }

        /// <summary>
        /// Creates a bucket if not found.
        /// </summary>
        /// <param name="bucket"></param>
        /// <returns></returns>
        public async Task CreateBucketIfNotExists(string bucket)
        {
            bool exists = await AmazonS3Util.DoesS3BucketExistAsync(_client, bucket);
            if (!exists)
            {
                await _client.PutBucketAsync(bucket);
            }
        }
        /// <summary>
        /// Deletes a bucket with everything under it.
        /// </summary>
        /// <param name="bucket"></param>
        /// <returns></returns>
        public async Task DeleteBucket(string bucket)
        {
            await AmazonS3Util.DeleteS3BucketWithObjectsAsync(_client, bucket);
        }

        /// <summary>
        /// Creates a share link to a file with a specified time period.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="expires"></param>
        /// <returns></returns>
        public async Task<string> GetShareLink(string bucket, string key, DateTime expires)
        {

            GetPreSignedUrlRequest request =
               new GetPreSignedUrlRequest()
               {
                   BucketName = bucket,
                   Key = key,
                   Expires = expires
               };

            return _client.GetPreSignedURL(request);
        }
    }
}
