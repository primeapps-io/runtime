using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using static PrimeApps.Util.Storage.UnifiedStorage;

namespace PrimeApps.Util.Storage
{
    public interface IUnifiedStorage
    {
        IAmazonS3 Client { get; }
        Task AbortMultipartUpload(string bucket, string key, string uploadId);
        Task<CompleteMultipartUploadResponse> CompleteMultipartUpload(string bucket, string key, string eTags, string finalETag, string uploadId);
        Task<CopyObjectResponse> CopyObject(string sourceBucket, string key, string destinationBucket, string destinationKey);
        Task<Amazon.S3.Model.ListObjectsResponse> GetListObject(string sourceBucket);
        Task CopyBucket(string sourceBucket, string destinationBucket, string[] withouts = null);
        Task CreateBucketIfNotExists(string bucket);
        Task DeleteBucket(string bucket);
        Task<DeleteObjectResponse> DeleteObject(string bucket, string key);
        Task<FileStreamResult> Download(string bucket, string key, string fileName);
        Task<string> InitiateMultipartUpload(string bucket, string key);
        Task UploadDirAsync(string bucket, string folderPath);
        Task Upload(string bucket, string key, Stream stream);
        Task Upload(PutObjectRequest request);
        Task Upload(string fileName, string bucket, string key, Stream stream);
        Task<string> UploadPart(string bucket, string key, int chunk, int chunks, string uploadId, Stream stream);
        string GetShareLink(string bucket, string key, DateTime expires, Protocol protocol = Protocol.HTTPS);
        string GetLink(string bucket, string key, string storageHostUrl = null);
        Task<GetObjectResponse> GetObject(string bucket, string key);
        Task<bool> FolderExists(string bucket);
        Task<bool> ObjectExists(string bucket, string key);
        Task<PutACLResponse> CreateACL(string bucket, string key, S3CannedACL cannedACL);
        Task AddHttpReferrerUrlToBucket(string bucketName, string url, PolicyType type);
        Task<PutBucketPolicyResponse> CreateBucketPolicy(string bucket, string domainName, PolicyType policyType, bool CreateBucketIfNotExists = true);
        event FileUploaded FileUploadedEvent;
        string GetDownloadFolderPath();
        Task<bool> DownloadFolder(string bucketName, string directory, string destinationPath);
        Task<bool> DownloadByPath(string bucketName, string key, string filePath);
        string GetDocUrl(GetPreSignedUrlRequest request);
    }
}