using Microsoft.AspNetCore.Mvc;
using Microsoft.Net.Http.Headers;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.RetryPolicies;
using System;
using System.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    /// <summary>
    /// Main storage class for the Webservice.
    /// This class is like an interface between Windows Azure Storage and our Webservice
    /// We use Windows Azure Blob Storage as storage provider and we are using BlockBlob class to store files.
    /// This method allows us to store files by chunks and then merge them together.
    /// Every instance(workgroup) has a seperate Windows Azure Cloud Blob Container.
    /// Also we use crmDocuments table to store some information about files, like file name, content-type, file size, user and instance on Windows Azure.
    /// For more information about Windows Azure Storage: http://www.windowsazure.com/en-us/documentation/services/storage/
    /// </summary>
    class Storage
    {
        /// <summary>
        /// This method allows to upload chunked files to the storage.
        /// </summary>
        /// <param name="chunk">Chunk count for the file</param>
        /// <param name="fileContent">Chunk stream</param>
        /// <param name="containerName">Container name for the block blob</param>
        /// <param name="tempName">Temporary file name</param>
        /// <param name="contentType">Content Type of file</param>
        public static void UploadFile(int chunk, Stream fileContent, string containerName, string tempName, string contentType, BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Blob)
        {
            //get/create blob container
            var blobContainer = GetBlobContainer(containerName, accessType);

            //create block id with the given chunk id.
            var blockId = Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", chunk)));

            //get  block blob reference for the file
            CloudBlockBlob tempBlob = blobContainer.GetBlockBlobReference(tempName);

            //seek file stream to the beginning.
            fileContent.Seek(0, SeekOrigin.Begin);

            //put block to the blob storage.
            tempBlob.PutBlockAsync(blockId, fileContent, null, AccessCondition.GenerateEmptyCondition(), new BlobRequestOptions() { RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(10), 3) }, new OperationContext());
        }

        /// <summary>
        /// This is a necessary method to call after file upload completed.
        /// This method commits the temporary blob to the permanent storage with it's real name.
        /// </summary>
        /// <param name="tempName">Temporary name of the uploaded file</param>
        /// <param name="newName">Real name of the uploaded file</param>
        /// <param name="contentType">Content Type for the uploaded file</param>
        /// <param name="containerName">Container name for the uploaded file</param>
        /// <param name="chunks">Total chunks for the uploaded file</param>
        /// <param name="tempblobContainerName">Temporary blob container name for the uploaded file to move/copy on selected container. Default 'temp'</param>
        public static CloudBlockBlob CommitFile(string tempName, string newName, string contentType, string containerName, int chunks, BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Blob, string tempblobContainerName = "temp", string relatedMetadataRecordIdForBlob = null, string relatedMetadataModuleNameForBlob = null, string relatedMetadataViewFileName = null, string relatedMetadataFullFileName = null)
        {
            //get temporary blob container which reserved for uploads.
            var tempBlobContainer = GetBlobContainer(tempblobContainerName);

            //get blob container for the instance
            var blobContainer = GetBlobContainer(containerName, accessType);

            //calculate chunk id's.
            var blockList = Enumerable.Range(0, chunks).ToList<int>().ConvertAll(rangeElement => Convert.ToBase64String(Encoding.UTF8.GetBytes(string.Format(CultureInfo.InvariantCulture, "{0:D4}", rangeElement))));

            //get temporary blob
            var tempBlob = tempBlobContainer.GetBlockBlobReference(tempName);

            //commit id's for the temporary blob
            tempBlob.PutBlockListAsync(blockList);

            //create a new blob with real file name
            var newBlob = blobContainer.GetBlockBlobReference(newName);

            if (relatedMetadataRecordIdForBlob != null)
            {
                newBlob.Metadata.Add("recordid", relatedMetadataRecordIdForBlob);
            }
            if (relatedMetadataModuleNameForBlob != null)
            {
                newBlob.Metadata.Add("module", relatedMetadataModuleNameForBlob);
            }

            if (relatedMetadataFullFileName != null)
            {
                newBlob.Metadata.Add("fullfilename", relatedMetadataFullFileName);
            }

            if (relatedMetadataViewFileName != null)
            {
                newBlob.Metadata.Add("viewfilename", relatedMetadataViewFileName);
            }
            //copy data from temprorary blob to  blob
            newBlob.StartCopyAsync(tempBlob);

            //set content type
            newBlob.Properties.ContentType = contentType;

            //apply properties
            newBlob.SetPropertiesAsync();

            //delete temprorary blob
            tempBlob.DeleteAsync();

            return newBlob;
        }

        /// <summary>
        /// Gets file reference as CloudBlob and allows to download it.
        /// </summary>
        /// <param name="containerName">Container(instance) name for the file</param>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        public static CloudBlockBlob GetBlob(string containerName, string fileName)
        {
            //get blob container
            var blobContainer = GetBlobContainer(containerName);

            //get blob
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(fileName);
            return blob;
        }

        /// <summary>
        /// Removes file from the blog storage
        /// </summary>
        /// <param name="containerName">Container(instance) name for the file</param>
        /// <param name="fileName">File name</param>
        public static void RemoveFile(string containerName, string fileName)
        {
            //get blob container
            var blobContainer = GetBlobContainer(containerName);

            //get blob
            CloudBlockBlob blob = blobContainer.GetBlockBlobReference(fileName);

            //delete the blob.
            blob.DeleteIfExistsAsync();
        }

        /// <summary>
        /// Gets blob container reference
        /// </summary>
        /// <param name="containerName">Container(instance) name for the file</param>
        /// <returns></returns>
        public static CloudBlobContainer GetBlobContainer(string containerName, BlobContainerPublicAccessType accessType = BlobContainerPublicAccessType.Blob)
        {
            // Variables for the cloud storage objects.
            CloudStorageAccount cloudStorageAccount;
            CloudBlobClient blobClient;
            CloudBlobContainer blobContainer;

            // Use the local storage account.
            cloudStorageAccount = CloudStorageAccount.Parse(ConfigurationManager.AppSettings.Get("Microsoft.WindowsAzure.Plugins.Storage.ConnectionString"));

            // create blob container
            blobClient = cloudStorageAccount.CreateCloudBlobClient();
            blobContainer = blobClient.GetContainerReference(containerName);
            blobContainer.CreateIfNotExistsAsync();


            return blobContainer;
        }

        /// <summary>
        /// Makes a copy of blob file
        /// </summary>
        /// <param name="containerName">Container(instance) name for the file</param>
        /// <param name="fileName">File name</param>
        /// <returns></returns>
        public static CopyStatus CopyBlob(ref string fileName, Guid containerName)
        {
            CloudBlobContainer sourceContainer = Storage.GetBlobContainer(string.Format("inst-{0}", containerName));
            CloudBlobContainer targetContainer = Storage.GetBlobContainer(string.Format("inst-{0}", containerName));

            string newFileName = Guid.NewGuid().ToString();

            if (fileName.LastIndexOf('.') > 0)
            {
                newFileName = fileName.Replace(fileName.Substring(0, fileName.LastIndexOf('.')), newFileName);
            }

            CloudBlockBlob sourceBlob = sourceContainer.GetBlockBlobReference(fileName);
            CloudBlockBlob targetBlob = targetContainer.GetBlockBlobReference(newFileName);
            var copyToken = targetBlob.StartCopyAsync(sourceBlob.Uri);

            fileName = newFileName;
            return targetBlob.CopyState.Status;
        }

        /// <summary>
        /// Gets avatar full url
        /// </summary>
        /// <returns></returns>
        public static string GetAvatarUrl(string avatar)
        {
            if (string.IsNullOrWhiteSpace(avatar))
                return string.Empty;

            var blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");

            return $"{blobUrl}/user-images/{avatar}";
        }

        public static string GetLogoUrl(string logo)
        {
            if (string.IsNullOrWhiteSpace(logo))
                return string.Empty;

            var blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");

            return $"{blobUrl}/app-logo/{logo}";
        }


        public static async Task<FileStreamResult> DownloadToFileStreamResult(CloudBlockBlob blob, string fileName)
        {
            Stream outputStream = null;

            await blob.DownloadToStreamAsync(outputStream, AccessCondition.GenerateEmptyCondition(), new BlobRequestOptions()
            {
                ServerTimeout = TimeSpan.FromDays(1),
                RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(10), 3)
            }, null);

            await blob.FetchAttributesAsync();
            FileStreamResult result = new FileStreamResult(outputStream, blob.Properties.ContentType)
            {
                FileDownloadName = fileName,
                LastModified = blob.Properties.LastModified,
                EntityTag = new EntityTagHeaderValue(blob.Properties.ETag)
            };
            return result;
        }

    }

}
