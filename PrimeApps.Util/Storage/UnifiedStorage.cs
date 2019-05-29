using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Text;

namespace PrimeApps.Util.Storage
{
    /// <summary>
    /// Unified Storage library based on Amazon S3
    /// </summary>
    public class UnifiedStorage : IUnifiedStorage
    {
        private IConfiguration _configuration;
        private IAmazonS3 _client;

        public IAmazonS3 Client
        {
            get { return _client; }
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
            NONE,
            APPLOGO,
            APPTEMPLATE,
            RELEASE
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
            {ObjectType.NONE, ""},
            {ObjectType.APPLOGO, "/app_logo/"},
            {ObjectType.APPTEMPLATE, "/app_template/"},
            {ObjectType.RELEASE, "/release/"}
        };




        const string HttpReferrerPolicy = "{" +
"  \"Version\":\"2012-10-17\"," +
"  \"Id\":\"{bucketName}_http_referrer\"," +
"  \"Statement\":[" +
"    {" +
"      \"Sid\":\"{bucketName}_http_referrer\"," +
"      \"Effect\":\"Allow\"," +
"      \"Principal\":\"*\"," +
"      \"Action\":\"s3:GetObject\"," +
"      \"Resource\":\"arn:aws:s3:::{bucketName}/*\"," +
"      \"Condition\":{" +
"        \"StringLike\":{\"aws:Referer\":[\"{domainName}/*\"]}" +
"      }" +
"    }" +
"  ]" +
"}";

        const string PublicReadPolicy = "{" +
"  \"Version\":\"2012-10-17\"," +
"  \"Id\":\"{bucketName}_public_read\"," +
"  \"Statement\":[" +
"    {" +
"      \"Sid\":\"{bucketName}_public_read\"," +
"      \"Effect\":\"Allow\"," +
"      \"Principal\": \"*\"," +
"      \"Action\":[\"s3:GetObject\"]," +
"      \"Resource\":[\"arn:aws:s3:::{bucketName}/*\"]" +
"    }" +
"  ]" +
"}";

        const string TenantPolicy = "{" +
"  \"Version\":\"2012-10-17\"," +
"  \"Id\":\"{bucketName}_policy\"," +
"  \"Statement\":[" +
"    {" +
"      \"Sid\":\"http_referrer\"," +
"      \"Effect\":\"Allow\"," +
"      \"Principal\":\"*\"," +
"      \"Action\":\"s3:GetObject\"," +
"      \"Resource\":\"arn:aws:s3:::{bucketName}/*\"," +
"      \"Condition\":{" +
"        \"StringLike\":{\"aws:Referer\":[\"{domainName}/*\"]}" +
"      }" +
"    }, " +
"    {" +
"      \"Sid\":\"public_read_email\"," +
"      \"Effect\":\"Allow\"," +
"      \"Principal\": \"*\"," +
"      \"Action\":[\"s3:GetObject\"]," +
"      \"Resource\":[\"arn:aws:s3:::{bucketName}/mail/*\"]" +
"    }" +
"  ]" +
"}";
        public enum PolicyType
        {
            HTTPReferrer,
            PublicRead,
            TenantPolicy
        }
        public UnifiedStorage(IAmazonS3 client, IConfiguration configuration)
        {
            _client = client;
            ((AmazonS3Config)(_client.Config)).ForcePathStyle = true;
            _configuration = configuration;
        }

        /// <summary>
        /// Uploads a file stream into a bucket.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="stream"></param>
        /// <returns></returns>
        public async Task UploadDirAsync(string bucket, string folderPath)
        {
            try
            {
                await CreateBucketIfNotExists(bucket);

                var directoryTransferUtility =
                    new TransferUtility(_client);

                // 1. Upload a directory.
                await directoryTransferUtility.UploadDirectoryAsync(folderPath, bucket);

                // 2. Upload only the .txt files from a directory 
                //    and search recursively. 
                /*await directoryTransferUtility.UploadDirectoryAsync(
                                               folderPath,
                                               bucket,
                                               "*",
                                               SearchOption.AllDirectories);*/

                // 3. The same as Step 2 and some optional configuration. 
                //    Search recursively for .txt files to upload.
                var request = new TransferUtilityUploadDirectoryRequest
                {
                    BucketName = bucket,
                    Directory = folderPath,
                    SearchOption = SearchOption.AllDirectories,
                    SearchPattern = "*"
                };

                await directoryTransferUtility.UploadDirectoryAsync(request);
                Console.WriteLine("Upload statement 3 completed");
            }
            catch (AmazonS3Exception e)
            {
                Console.WriteLine(
                        "Error encountered ***. Message:'{0}' when writing an object", e.Message);
            }
            catch (Exception e)
            {
                Console.WriteLine(
                    "Unknown encountered on server. Message:'{0}' when writing an object", e.Message);
            }
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

        public async Task Upload(string fileName, string bucket, string key, Stream stream)
        {
            await Upload(bucket, key, stream);
            if (FileUploadedEvent != null)
            {
                FileUploadedEvent(bucket, key, fileName);
            }
        }

        // public async Task UploadWith(string fileName, string bucket, string key, Stream stream)
        // {
        //     await CreateBucketIfNotExists(bucket);

        //     using (TransferUtility transUtil = new TransferUtility(_client))
        //     {
        //         await transUtil.UploadAsync(stream, bucket, key);
        //         var currenUser = CurrentUser;

        //         var email = _context?.HttpContext?.User?.FindFirst("email").Value;
        //         _queue.QueueBackgroundWorkItem(token => _historyHelper.Storage(fileName, key, "PUT", bucket, email, currenUser));
        //     }
        // }

        public event FileUploaded FileUploadedEvent;
        public delegate void FileUploaded(string bucket, string key, string fileName);

        /// <summary>
        /// Initiates multipart upload.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <returns>Upload id required to upload parts.</returns>
        public async Task<string> InitiateMultipartUpload(string bucket, string key)
        {
            await CreateBucketIfNotExists(bucket);

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
        public async Task<string> UploadPart(string bucket, string key, int chunk, int chunks, string uploadId,
            Stream stream)
        {
            UploadPartResponse response;

            UploadPartRequest uploadRequest = new UploadPartRequest
            {
                BucketName = bucket,
                Key = key,
                UploadId = uploadId,
                PartNumber = chunk,
                InputStream = stream,
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

        public async Task<CompleteMultipartUploadResponse> CompleteMultipartUpload(string bucket, string key,
            string eTags, string finalETag, string uploadId)
        {
            eTags += string.IsNullOrWhiteSpace(eTags) ? finalETag : $"|{finalETag}";
            List<PartETag> eTagList = string.IsNullOrWhiteSpace(eTags.ToString())
                ? new List<PartETag>()
                : eTags.ToString().Split("|").Select((x, i) => new PartETag(i + 1, x)).ToList();

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
        /// Creates ACL for the object.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="cannedACL"></param>
        /// <returns>PutACLResponse</returns>
        public async Task<PutACLResponse> CreateACL(string bucket, string key, S3CannedACL cannedACL)
        {
            PutACLRequest request = new PutACLRequest()
            {
                CannedACL = cannedACL,
                BucketName = bucket,
                Key = key
            };

            return await _client.PutACLAsync(request);
        }

        public async Task<PutBucketPolicyResponse> CreateBucketPolicy(string bucket, string domainName, PolicyType policyType, bool createBucketIfNotExists = true)
        {
            string policy = string.Empty;

            switch (policyType)
            {
                case PolicyType.HTTPReferrer:
                    policy = HttpReferrerPolicy;
                    break;
                case PolicyType.PublicRead:
                    policy = PublicReadPolicy;
                    break;
                case PolicyType.TenantPolicy:
                    policy = TenantPolicy;
                    break;
            }

            await CreateBucketIfNotExists(bucket);

            string topBucketName = bucket.Split("/").FirstOrDefault();

            policy = policy.Replace("{domainName}", domainName).Replace("{bucketName}", bucket);

            PutBucketPolicyRequest putRequest = new PutBucketPolicyRequest
            {
                BucketName = topBucketName,
                Policy = policy
            };

            return await _client.PutBucketPolicyAsync(putRequest);
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
            string[] paths = bucket.Split('/');
            string checkPath = "";

            foreach (string path in paths)
            {
                checkPath += $"{path}/";
                bool exists = await AmazonS3Util.DoesS3BucketExistAsync(_client, checkPath);
                if (!exists)
                {
                    await _client.PutBucketAsync(checkPath);
                }
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
        /// <param name="protocol"></param>
        /// <returns></returns>
        public string GetLink(string bucket, string key, string storageHostUrl = null)
        {
            string storageUrl = string.Empty;
            if (bucket.EndsWith('/'))
                bucket = bucket.Remove(bucket.Length - 1, 1);
            if (!string.IsNullOrEmpty(storageHostUrl))
            {
                storageUrl = $"{storageHostUrl}/{bucket}/{key}";
            }
            else
            {
                storageUrl = $"{bucket}/{key}";
            }

            return storageUrl;
        }

        /// <summary>
        /// Creates a share link to a file with a specified time period.
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <param name="expires"></param>
        /// <param name="protocol"></param>
        /// <returns></returns>
        public string GetShareLink(string bucket, string key, DateTime expires, Protocol protocol = Protocol.HTTP)
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
        public async Task<CopyObjectResponse> CopyObject(string sourceBucket, string key, string destinationBucket,
            string destinationKey)
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
        /// Copies buckets with all objects.
        /// </summary>
        /// <param name="sourceBucket"></param>
        /// <param name="destinationBucket"></param>
        /// <param name="isRecursive"></param>
        public async Task CopyBucket(string sourceBucket, string destinationBucket)
        {
            await CreateBucketIfNotExists(destinationBucket);
            var listOfObjects = await _client.ListObjectsAsync(sourceBucket);
            foreach (var obj in listOfObjects.S3Objects)
            {
                await CopyObject(sourceBucket, obj.Key, destinationBucket, obj.Key);
            }
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

        public async Task<PutLifecycleConfigurationResponse> SetLifeCycle(string bucket, int days)
        {
            LifecycleConfiguration config = new LifecycleConfiguration();
            config.Rules.Add(new LifecycleRule()
            {
                Expiration = new LifecycleRuleExpiration()
                {
                    Days = days
                }
            });

            PutLifecycleConfigurationRequest request = new PutLifecycleConfigurationRequest
            {
                BucketName = bucket,
                Configuration = config
            };

            return await _client.PutLifecycleConfigurationAsync(request);
        }

        public async Task<GetObjectResponse> GetObject(string bucket, string key)
        {
            return await _client.GetObjectAsync(new GetObjectRequest()
            {
                BucketName = bucket,
                Key = key
            });
        }

        /// <summary>
        /// Checks if file(object) exists
        /// </summary>
        /// <param name="bucket"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> ObjectExists(string bucket, string key)
        {
            var response = await _client.GetAllObjectKeysAsync(bucket, key, null);
            return response.Count > 0;
        }

        public static string GetMimeType(string name)
        {
            var type = name.Split('.')[1];
            switch (type)
            {
                case "gif":
                    return "image/bmp";
                case "bmp":
                    return "image/bmp";
                case "jpeg":
                case "jpg":
                    return "image/jpeg";
                case "png":
                    return "image/png";
                case "tif":
                case "tiff":
                    return "image/tiff";
                case "doc":
                    return "application/msword";
                case "docx":
                    return "application/vnd.openxmlformats-officedocument.wordprocessingml.document";
                case "pdf":
                    return "application/pdf";
                case "ppt":
                    return "application/vnd.ms-powerpoint";
                case "pptx":
                    return "application/vnd.openxmlformats-officedocument.presentationml.presentation";
                case "xlsx":
                    return "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                case "xls":
                    return "application/vnd.ms-excel";
                case "csv":
                    return "text/csv";
                case "xml":
                    return "text/xml";
                case "txt":
                    return "text/plain";
                case "zip":
                    return "application/zip";
                case "ogg":
                    return "application/ogg";
                case "mp3":
                    return "audio/mpeg";
                case "wma":
                    return "audio/x-ms-wma";
                case "wav":
                    return "audio/x-wav";
                case "wmv":
                    return "audio/x-ms-wmv";
                case "swf":
                    return "application/x-shockwave-flash";
                case "avi":
                    return "video/avi";
                case "mp4":
                    return "video/mp4";
                case "mpeg":
                    return "video/mpeg";
                case "mpg":
                    return "video/mpeg";
                case "qt":
                    return "video/quicktime";
                default:
                    return "image/jpeg";
            }
        }


        public static string GetPath(string type, int? tenant = null, int? appId = null, string extraPath = "")
        {
            ObjectType objectType = (ObjectType)System.Enum.Parse(typeof(ObjectType), type, true);

            if (appId == null)
                return $"tenant{tenant}{pathMap[objectType]}{extraPath}";
            else
                return $"app{appId}{pathMap[objectType]}{extraPath}";
        }

        public static string GetPathPictures(string type, int userId, string extraPath = "")
        {
            ObjectType objectType = (ObjectType)System.Enum.Parse(typeof(ObjectType), type, true);

            return $"profile-pictures{pathMap[objectType]}{"user" + userId}{extraPath}";
        }
        public static string GetPathComponents(string folderName, string componentName)
        {
            return $"components/{folderName}/{componentName}";
        }

        public static ObjectType GetType(string type)
        {
            return (ObjectType)System.Enum.Parse(typeof(ObjectType), type, true);
        }
    }
}