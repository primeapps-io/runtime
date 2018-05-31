using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.App.Helpers;
using PrimeApps.App.Results;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Document = PrimeApps.Model.Entities.Application.Document;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Helpers.QueryTranslation;
using Aspose.Words;
using MimeMapping;
using Aspose.Words.MailMerging;
using PrimeApps.App.Extensions;
using Microsoft.WindowsAzure.Storage.Blob;
using PrimeApps.App.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using PrimeApps.App.Storage.Unified;
using Amazon.S3.Model;

namespace PrimeApps.App.Controllers
{
    [Route("api/storage"), Authorize]
    public class StorageController : BaseController
    {
        private IDocumentRepository _documentRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private ITemplateRepostory _templateRepository;
        private INoteRepository _noteRepository;
        private ISettingRepository _settingRepository;
        private IUnifiedStorage _storage;
        public StorageController(IDocumentRepository documentRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, ITemplateRepostory templateRepository, INoteRepository noteRepository, IPicklistRepository picklistRepository, ISettingRepository settingRepository, IUnifiedStorage storage)
        {
            _documentRepository = documentRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _templateRepository = templateRepository;
            _noteRepository = noteRepository;
            _settingRepository = settingRepository;
            _storage = storage;
        }

        /// <summary>
        /// Uploads files by the chucks as a stream.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <returns>System.String.</returns>
        [HttpPost("upload")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> Upload(IFormCollection form)
        {

            IFormFile file = form.Files.First();
            StringValues bucketName = $"tenant{AppUser.TenantId}",
                chunksStr, uploadId, chunkStr, fileName, responseList;

            form.TryGetValue("chunks", out chunksStr);
            form.TryGetValue("chunk", out chunkStr);
            form.TryGetValue("name", out fileName);
            form.TryGetValue("uploadId", out uploadId);
            form.TryGetValue("responseList", out responseList);

            int chunk = 1,
                chunks = 1;

            int.TryParse(chunksStr, out chunks);
            int.TryParse(chunkStr, out chunk);
            chunk++; // increase it since s3 chunk indexes starting from 1 instead of 0

            MultipartResponse response = new MultipartResponse();
            CompleteMultipartUploadResponse uploadResult;
            try
            {
                if (chunk == 1)
                {
                    uploadId = await _storage.InitiateMultipartUpload(bucketName, fileName);
                    response.Status = MultipartStatusEnum.Initiated;
                    response.UploadId = uploadId;
                }

                response.ETag = await _storage.UploadPart(bucketName, fileName, chunk, chunks, uploadId, file.OpenReadStream());
                if (response.Status == MultipartStatusEnum.NotSet)
                    response.Status = MultipartStatusEnum.ChunkUpload;

                if (chunk == chunks)
                {
                    uploadResult = await _storage.CompleteMultipartUpload(bucketName, fileName, responseList, response.ETag, uploadId);
                    response.Status = MultipartStatusEnum.Completed;
                }
            }
            catch (Exception)
            {
                await _storage.AbortMultipartUpload(bucketName, fileName, uploadId);
                response.Status = MultipartStatusEnum.Aborted;
            }

            return Json(response);
        }
        /// <summary>
        /// Using at email attachments and on module 
        /// </summary>
        /// <returns></returns>
        [Route("upload_attachment"), HttpPost]
        public async Task<IActionResult> UploadAttachment()
        {
            //Parse stream and get file properties.
            HttpMultipartParser parser = new HttpMultipartParser(Request.Body, "file");
            String blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");
            //if it is successfully parsed continue.
            if (parser.Success)
            {

                if (parser.FileContents.Length <= 0)
                {
                    //check the file size if it is 0 bytes then return client with that error code.
                    return BadRequest();
                }

                //declare chunk variables
                int chunk = 0; //current chunk
                int chunks = 1; //total chunk count

                string uniqueName = string.Empty;
                string container = string.Empty;

                //if parser has more then 1 parameters, it means that request is chunked.
                if (parser.Parameters.Count > 1)
                {
                    //calculate chunk variables.
                    chunk = int.Parse(parser.Parameters["chunk"]);
                    chunks = int.Parse(parser.Parameters["chunks"]);

                    //get the file name from parser
                    if (parser.Parameters.ContainsKey("name"))
                        uniqueName = parser.Parameters["name"];

                    if (parser.Parameters.ContainsKey("container"))
                        container = parser.Parameters["container"];
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;
                }

                //send stream and parameters to storage upload helper method for temporary upload.
                AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType);

                var result = new DocumentUploadResult();
                result.ContentType = parser.ContentType;
                result.UniqueName = uniqueName;

                if (chunk == chunks - 1)
                {
                    CloudBlockBlob blob = AzureStorage.CommitFile(uniqueName, $"{container}/{uniqueName}", parser.ContentType, "pub", chunks);
                    result.PublicURL = $"{blobUrl}{blob.Uri.AbsolutePath}";
                }

                //return content type of the file to the client
                return Ok(result);
            }

            //this request invalid because there is no file, return fail code to the client.
            return NotFound();
        }
        /// <summary>
        /// Using at record document type upload for single file
        /// </summary>
        /// <returns></returns>
        [Route("upload_document_file"), HttpPost]
        public async Task<IActionResult> UploadDocumentFile()
        {
            //Parse stream and get file properties.
            HttpMultipartParser parser = new HttpMultipartParser(Request.Body, "file");
            String blobUrl = ConfigurationManager.AppSettings.Get("BlobUrl");
            //if it is successfully parsed continue.
            if (parser.Success)
            {

                if (parser.FileContents.Length <= 0)
                {
                    //check the file size if it is 0 bytes then return client with that error code.
                    return BadRequest();
                }

                if (Utils.BytesToMegabytes(parser.FileContents.Length) > 5) // 5 MB maximum
                {
                    return BadRequest();
                }


                string uniqueName = string.Empty;
                string fileName = string.Empty;
                string container = string.Empty;
                string moduleName = string.Empty;
                string fullFileName = string.Empty;
                string recordId = string.Empty;
                string documentSearch = string.Empty;
                bool documentSearchFlag = false;
                int uniqueRecordId = 0;

                //if parser has more then 1 parameters, it means that request is full filled by request maker object - uploader.
                if (parser.Parameters.Count > 1)
                {

                    //get the file name from parser
                    if (parser.Parameters.ContainsKey("name"))
                        uniqueName = parser.Parameters["name"];

                    if (parser.Parameters.ContainsKey("name"))
                        uniqueName = parser.Parameters["name"];

                    if (parser.Parameters.ContainsKey("filename"))
                        fileName = parser.Parameters["filename"];

                    if (parser.Parameters.ContainsKey("container"))
                        container = parser.Parameters["container"];

                    if (parser.Parameters.ContainsKey("modulename"))
                    {
                        moduleName = parser.Parameters["modulename"];
                        moduleName = moduleName.Replace("_", "-");
                    }




                    if (parser.Parameters.ContainsKey("documentsearch"))
                    {
                        documentSearch = parser.Parameters["documentsearch"];
                        bool.TryParse(documentSearch, out documentSearchFlag);
                    }
                    if (parser.Parameters.ContainsKey("recordid"))
                    {
                        recordId = parser.Parameters["recordid"];
                        int.TryParse(recordId, out uniqueRecordId);
                    }


                }

                if (string.IsNullOrEmpty(uniqueName) || string.IsNullOrEmpty(container) || string.IsNullOrEmpty(moduleName) || string.IsNullOrEmpty(fileName) || uniqueRecordId == 0)
                {
                    return BadRequest();
                }

                var ext = Path.GetExtension(parser.Filename);
                fullFileName = uniqueRecordId + "_" + uniqueName + ext;

                if (ext != ".pdf" && ext != ".txt" && ext != ".doc" && ext != ".docx")
                {
                    return BadRequest();
                }


                var chunk = 0;
                var chunks = 1; //one part chunk

                //send stream and parameters to storage upload helper method for temporary upload.
                AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", fullFileName, parser.ContentType);

                var result = new DocumentUploadResult();
                result.ContentType = parser.ContentType;
                result.UniqueName = fullFileName;


                CloudBlockBlob blob = AzureStorage.CommitFile(fullFileName, $"{container}/{moduleName}/{fullFileName}", parser.ContentType, "module-documents", chunks, BlobContainerPublicAccessType.Blob, "temp", uniqueRecordId.ToString(), moduleName, fileName, fullFileName);
                result.PublicURL = $"{blobUrl}{blob.Uri.AbsolutePath}";


                if (documentSearchFlag == true)
                {
                    var documentSearchHelper = new DocumentSearch();

                    documentSearchHelper.CreateOrUpdateIndexOnDocumentBlobStorage(AppUser.TenantGuid.ToString(), moduleName, false);//False because! 5 min auto index incremental change detection policy check which azure provided

                }

                //return content type of the file to the client
                return Ok(result);
            }

            //this request invalid because there is no file, return fail code to the client.
            return NotFound();
        }
        [Route("remove_document"), HttpPost]
        public async Task<IActionResult> RemoveModuleDocument([FromBody]JObject data)
        {
            string tenantId,
                module,
                recordId,
                fieldName,
                fileNameExt;
            module = data["module"]?.ToString();
            recordId = data["recordId"]?.ToString();
            fieldName = data["fieldName"]?.ToString();
            fileNameExt = data["fileNameExt"]?.ToString();
            tenantId = data["tenantId"]?.ToString();

            if (int.Parse(tenantId) != AppUser.TenantId)
            {
                //if instance id does not belong to current session, stop the request and send the forbidden status code.
                return Forbid();
            }

            string moduleDashesName = module.Replace("_", "-");

            var containerName = "module-documents";
            //var ext = Path.GetExtension(fileName);

            string uniqueFileName = $"{AppUser.TenantGuid}/{moduleDashesName}/{recordId}_{fieldName}.{fileNameExt}";

            //remove document
            AzureStorage.RemoveFile(containerName, uniqueFileName);



            var moduleData = await _moduleRepository.GetByNameBasic(module);
            var field = moduleData.Fields.FirstOrDefault(x => x.Name == fieldName);
            if (field.DocumentSearch)
            {
                DocumentSearch documentSearch = new DocumentSearch();
                documentSearch.CreateOrUpdateIndexOnDocumentBlobStorage(tenantId, module, false);
            }

            return Ok();
        }
        /// <summary>
        /// Downloads the file from cloud to client as a stream.
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        [Route("download"), HttpGet]
        public async Task<IActionResult> Download([FromRoute] int fileID)
        {
            //get the document record from database
            var doc = await _documentRepository.GetById(fileID);
            string publicName = "";

            if (doc != null)
            {
                //if there is a document with this id, try to get it from blob AzureStorage.
                var blob = AzureStorage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), doc.UniqueName);
                try
                {
                    //try to get the attributes of blob.
                    await blob.FetchAttributesAsync();
                }
                catch (Exception)
                {
                    //if there is an exception, it means there is no such file.
                    return NotFound();
                }

                //Is bandwidth enough to download this file?
                //Bandwidth is enough, send the AzureStorage.
                publicName = doc.Name;


                //return new FileDownloadResult()
                //{
                //    Blob = blob,
                //    PublicName = doc.Name
                //};

                //try
                //{
                //await Blob.DownloadToStreamAsync(outputStream, AccessCondition.GenerateEmptyCondition(), new BlobRequestOptions()
                //{
                //    ServerTimeout = TimeSpan.FromDays(1),
                //    RetryPolicy = new LinearRetry(TimeSpan.FromSeconds(10), 3)
                //}, null);
                //}
                //finally
                //{
                //    outputStream.Close();
                //}


                return await AzureStorage.DownloadToFileStreamResultAsync(blob, publicName);
            }
            else
            {
                //there is no such file, return
                return NotFound();
            }
        }

        /// <summary>
        /// Downloads the file from cloud to client as a stream.
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        [Route("download_module_document"), HttpGet]
        public async Task<IActionResult> DownloadModuleDocument([FromRoute] string module, [FromRoute] string fileName, [FromRoute] string fileNameExt, [FromRoute] string fieldName, [FromRoute] string recordId)
        {
            var publicName = "";

            if (!string.IsNullOrEmpty(module) && !string.IsNullOrEmpty(fileNameExt) && !string.IsNullOrEmpty(fieldName))
            {

                var containerName = "module-documents";
                //var ext = Path.GetExtension(fileName);

                module = module.Replace("_", "-");

                string uniqueFileName = $"{AppUser.TenantGuid}/{module}/{recordId}_{fieldName}.{fileNameExt}";
                var docName = fileName + "." + fileNameExt;

                //if there is a document with this id, try to get it from blob AzureStorage.
                var blob = AzureStorage.GetBlob(containerName, uniqueFileName);
                try
                {
                    //try to get the attributes of blob.
                    await blob.FetchAttributesAsync();
                }
                catch (Exception)
                {
                    //if there is an exception, it means there is no such file.
                    return NotFound();
                }
                publicName = docName;

                return await AzureStorage.DownloadToFileStreamResultAsync(blob, publicName);

            }
            else
            {
                //there is no such file, return
                return BadRequest();
            }
        }

        /// <summary>
        /// Removes the document from database and blob AzureStorage.
        /// </summary>
        /// <param name="DocID">The document identifier.</param>
        [Route("Remove"), HttpPost]
        public async Task<IActionResult> Remove(DocumentDTO doc)
        {
            //if the document is not null open a new session and transaction
            var result = await _documentRepository.RemoveAsync(doc.ID);
            if (result != null)
            {
                //Update storage license for instance.
                return Ok();
            }


            return NotFound();
        }

        [Route("download_template"), HttpGet]
        public async Task<IActionResult> DownloadTemplate([FromRoute]int templateId)
        {
            //get the document record from database
            var template = await _templateRepository.GetById(templateId);
            string publicName = "";

            if (template != null)
            {
                //if there is a document with this id, try to get it from blob AzureStorage.
                var blob = AzureStorage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), $"templates/{template.Content}");
                try
                {
                    //try to get the attributes of blob.
                    await blob.FetchAttributesAsync();
                }
                catch (Exception)
                {
                    //if there is an exception, it means there is no such file.
                    return NotFound();
                }

                //Bandwidth is enough, send the AzureStorage.
                publicName = template.Name;

                string[] splittedFileName = template.Content.Split('.');
                string extension = splittedFileName.Length > 1 ? splittedFileName[1] : "docx";

                return await AzureStorage.DownloadToFileStreamResultAsync(blob, $"{template.Name}.{extension}");

            }
            else
            {
                //there is no such file, return
                return NotFound();
            }
        }
    }
}
