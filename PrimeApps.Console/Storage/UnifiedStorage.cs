using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.Console.Storage
{
    /// <summary>
    /// Unified Storage library based on Amazon S3
    /// </summary>
    public class UnifiedStorage : IUnifiedStorage
    {
        private IAmazonS3 _client;
        public IAmazonS3 Client { get { return _client; } }

        public UnifiedStorage(IAmazonS3 client)
        {
            _client = client;
            ((AmazonS3Config)(_client.Config)).ForcePathStyle = true;
        }

        public enum ObjectType
        {
            MAIL,
            ATTACHMENT,
            RECORD,
            TEMPLATE,
            ANALYTIC,
            IMPORT,
            NOTE,
            LOGO,
            PROFILEPICTURE,
            NONE
        }

        static readonly Dictionary<ObjectType, string> pathMap = new Dictionary<ObjectType, string>
        {
            {ObjectType.ATTACHMENT, "/attachments/"},
            {ObjectType.RECORD, "/records/"},
            {ObjectType.TEMPLATE, "/templates/"},
            {ObjectType.ANALYTIC, "/analytics/"},
            {ObjectType.IMPORT, "/imports/"},
            {ObjectType.NOTE, "/notes/"},
            {ObjectType.LOGO, "/logos/"},
            {ObjectType.MAIL, "/mail/"},
            {ObjectType.PROFILEPICTURE, "/profile_pictures/"},
            {ObjectType.NONE, ""}
        };

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
        /// Initiates multipart upload.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <returns>Upload id required to upload parts.</returns>
        public async Task<string> InitiateMultipartUpload(string bucket, string key)
        {

            bool exists = await AmazonS3Util.DoesS3BucketExistAsync(_client, bucket);
            if (!exists)
            {
                await _client.PutBucketAsync(bucket);
            }

            // initiate if it is first chunk.
            var initialResult = await _client.InitiateMultipartUploadAsync(bucket, key);
            return initialResult.UploadId;
        }

        /// <summary>
        /// Uploads a multipart file stream into a bucket.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="chunk"></param>
        /// <param name="chunks"></param>
        /// <param name="eTags"></param>
        /// <param name="uploadId"></param>
        /// <param name="stream"></param>
        /// <returns>ETag for the uploaded file part</returns>
        public async Task<string> UploadPart(string bucket, string key, int chunk, int chunks, string uploadId, Stream stream)
        {
            UploadPartResponse response;

            UploadPartRequest uploadRequest = new UploadPartRequest
            {
                BucketName = bucket,
                Key = key,
                UploadId = uploadId,
                PartNumber = chunk,
                InputStream = stream
            };

            // Upload a part
            response = await _client.UploadPartAsync(uploadRequest);

            return response.ETag;
        }

        /// <summary>
        /// Aborts multipart upload request.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="uploadId"></param>
        /// <returns></returns>
        public async Task AbortMultipartUpload(string bucket, string key, string uploadId)
        {
            // Abort the upload.
            AbortMultipartUploadRequest abortMPURequest = new AbortMultipartUploadRequest
            {
                BucketName = bucket,
                Key = key,
                UploadId = uploadId
            };

            await _client.AbortMultipartUploadAsync(abortMPURequest);
        }

        public async Task<CompleteMultipartUploadResponse> CompleteMultipartUpload(string bucket, string key, string eTags, string finalETag, string uploadId)
        {
            eTags += string.IsNullOrWhiteSpace(eTags) ? finalETag : $"|{finalETag}";
            List<PartETag> eTagList = string.IsNullOrWhiteSpace(eTags.ToString()) ? new List<PartETag>() : eTags.ToString().Split("|").Select((x, i) => new PartETag(i + 1, x)).ToList();

            // Setup to complete the upload.
            CompleteMultipartUploadRequest completeRequest = new CompleteMultipartUploadRequest
            {
                BucketName = bucket,
                Key = key,
                UploadId = uploadId,
                PartETags = eTagList
            };

            return await _client.CompleteMultipartUploadAsync(completeRequest);
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
        public string GetShareLink(string bucket, string key, DateTime expires, Protocol protocol = Protocol.HTTP, bool clearRoot = true)
        {
            if (bucket.EndsWith('/'))
                bucket = bucket.Remove(bucket.Length - 1, 1);

            GetPreSignedUrlRequest request =
                new GetPreSignedUrlRequest()
                {
                    BucketName = bucket,
                    Key = key,
                    Expires = expires,
                    Protocol = protocol
                };

            var preSignedUrl = _client.GetPreSignedURL(request);

            return preSignedUrl;
        }


        /// <summary>
        /// Copies objects from one bucket to another.
        /// </summary>
        /// <param name="sourceBucket"></param>
        /// <param name="key"></param>
        /// <param name="destinationBucket"></param>
        /// <param name="destinationKey"></param>
        /// <returns></returns>
        public async Task<CopyObjectResponse> CopyObject(string sourceBucket, string key, string destinationBucket, string destinationKey)
        {
            CopyObjectRequest request = new CopyObjectRequest
            {
                SourceBucket = sourceBucket,
                SourceKey = key,
                DestinationBucket = destinationBucket,
                DestinationKey = destinationKey
            };
            return await _client.CopyObjectAsync(request);
        }
        /// <summary>
        /// Deletes an object from a bucket.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<DeleteObjectResponse> DeleteObject(string bucket, string key)
        {
            DeleteObjectRequest request = new DeleteObjectRequest
            {
                BucketName = bucket,
                Key = key
            };
            return await _client.DeleteObjectAsync(request);
        }

        public static string GetPath(string type, int tenant, string extraPath = "")
        {
            ObjectType objectType = (ObjectType)System.Enum.Parse(typeof(ObjectType), type, true);

            return $"tenant{tenant}{pathMap[objectType]}{extraPath}";
        }

        public static ObjectType GetType(string type)
        {
            return (ObjectType)System.Enum.Parse(typeof(ObjectType), type, true);
        }
    }
}
