using System;
using System.IO;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using static PrimeApps.Studio.Storage.UnifiedStorageOld;

namespace PrimeApps.Studio.Storage
{
    public interface IUnifiedStorageOld
    {
        Task AbortMultipartUpload(string bucket, string key, string uploadId);
        Task<CompleteMultipartUploadResponse> CompleteMultipartUpload(string bucket, string key, string eTags, string finalETag, string uploadId);
        Task<CopyObjectResponse> CopyObject(string sourceBucket, string key, string destinationBucket, string destinationKey);
        Task CreateBucketIfNotExists(string bucket);
        Task DeleteBucket(string bucket);
        Task<DeleteObjectResponse> DeleteObject(string bucket, string key);
        Task<FileStreamResult> Download(string bucket, string key, string fileName);
        string GetShareLink(string bucket, string key, DateTime expires, Protocol protocol = Protocol.HTTP, bool clearRoot = true);
        Task<PutBucketPolicyResponse> CreateBucketPolicy(string bucket, string domainName, PolicyType policyType);
        Task<string> InitiateMultipartUpload(string bucket, string key);
        Task Upload(string fileName, string bucket, string key, Stream stream);
        Task<string> UploadPart(string bucket, string key, int chunk, int chunks, string uploadId, Stream stream);
    }
}