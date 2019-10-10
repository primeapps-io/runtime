using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.App.Storage;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Document = PrimeApps.Model.Entities.Tenant.Document;

namespace PrimeApps.App.Controllers
{
    [Route("api/document"), Authorize]
    public class DocumentController : ApiBaseController
    {
        private IDocumentRepository _documentRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private ITemplateRepository _templateRepository;
        private INoteRepository _noteRepository;
        private IPicklistRepository _picklistRepository;
        private ISettingRepository _settingRepository;
        private IConfiguration _configuration;
        private IDocumentHelper _documentHelper;
        private IRecordHelper _recordHelper;
        private IUnifiedStorage _storage;

        public DocumentController(IDocumentRepository documentRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, ITemplateRepository templateRepository, INoteRepository noteRepository, IPicklistRepository picklistRepository, ISettingRepository settingRepository, IRecordHelper recordHelper, IConfiguration configuration, IDocumentHelper documentHelper, IUnifiedStorage storage)
        {
            _documentRepository = documentRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _templateRepository = templateRepository;
            _noteRepository = noteRepository;
            _picklistRepository = picklistRepository;
            _settingRepository = settingRepository;
            _configuration = configuration;
            _documentHelper = documentHelper;
            _recordHelper = recordHelper;
            _storage = storage;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_documentRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_templateRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_noteRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_picklistRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_settingRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        /// <summary>
        /// Uploads files by the chucks as a stream.
        /// </summary>
        /// <param name="fileContents">The file contents.</param>
        /// <returns>System.String.</returns>
        [Route("Upload"), HttpPost]
        public IActionResult Upload()
        {
            DocumentUploadResult result;
            var isUploaded = _documentHelper.Upload(Request.Body, out result);

            if (!isUploaded && result == null)
            {
                return NotFound();
            }

            if (!isUploaded)
            {
                return BadRequest();
            }

            return Ok(result);
        }

        [Route("Upload_Excel"), HttpPost]
        public IActionResult UploadExcel()
        {
            DocumentUploadResult result;
            var isUploaded = _documentHelper.UploadExcel(Request.Body, out result);

            if (!isUploaded && result == null)
            {
                return NotFound();
            }

            if (!isUploaded)
            {
                return BadRequest();
            }

            return Ok(result);
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
            String blobUrl = _configuration.GetValue("AppSettings:StorageUrl", string.Empty);
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
                    {
                        uniqueName = parser.Parameters["name"];
                    }

                    if (parser.Parameters.ContainsKey("container"))
                    {
                        container = parser.Parameters["container"];
                    }
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;
                }

                //send stream and parameters to storage upload helper method for temporary upload.
                AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType, _configuration).Wait();

                var result = new DocumentUploadResult();
                result.ContentType = parser.ContentType;
                result.UniqueName = uniqueName;

                if (chunk == chunks - 1)
                {
                    CloudBlockBlob blob = await AzureStorage.CommitFile(uniqueName, $"{container}/{uniqueName}", parser.ContentType, "pub", chunks, _configuration);
                    if (!string.IsNullOrEmpty(blobUrl))
                    {
                        result.PublicURL = $"{blobUrl}{blob.Uri.AbsolutePath}";
                    }
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
            String blobUrl = _configuration.GetValue("AppSettings:StorageUrl", string.Empty);
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
                    {
                        uniqueName = parser.Parameters["name"];
                    }

                    if (parser.Parameters.ContainsKey("name"))
                    {
                        uniqueName = parser.Parameters["name"];
                    }

                    if (parser.Parameters.ContainsKey("filename"))
                    {
                        fileName = parser.Parameters["filename"];
                    }

                    if (parser.Parameters.ContainsKey("container"))
                    {
                        container = parser.Parameters["container"];
                    }

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
                await AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", fullFileName, parser.ContentType, _configuration);

                var result = new DocumentUploadResult();
                result.ContentType = parser.ContentType;
                result.UniqueName = fullFileName;


                CloudBlockBlob blob = await AzureStorage.CommitFile(fullFileName, $"{container}/{moduleName}/{fullFileName}", parser.ContentType, "module-documents", chunks, _configuration, BlobContainerPublicAccessType.Blob, "temp", uniqueRecordId.ToString(), moduleName, fileName, fullFileName);
                if (!string.IsNullOrEmpty(blobUrl))
                {
                    result.PublicURL = $"{blobUrl}{blob.Uri.AbsolutePath}";
                }

                if (documentSearchFlag == true)
                {
                    var documentSearchHelper = new DocumentSearch();

                    documentSearchHelper.CreateOrUpdateIndexOnDocumentBlobStorage(AppUser.TenantGuid.ToString(), moduleName, _configuration, false); //False because! 5 min auto index incremental change detection policy check which azure provided
                }

                //return content type of the file to the client
                return Ok(result);
            }

            //this request invalid because there is no file, return fail code to the client.
            return NotFound();
        }

        [Route("remove_document"), HttpPost]
        public async Task<IActionResult> RemoveModuleDocument([FromBody] JObject data)
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
            AzureStorage.RemoveFile(containerName, uniqueFileName, _configuration);


            var moduleData = await _moduleRepository.GetByNameBasic(module);
            var field = moduleData.Fields.FirstOrDefault(x => x.Name == fieldName);
            if (field.DocumentSearch)
            {
                DocumentSearch documentSearch = new DocumentSearch();
                documentSearch.CreateOrUpdateIndexOnDocumentBlobStorage(tenantId, module, _configuration, false);
            }

            return Ok();
        }

        [Route("Create"), HttpPost]
        /// <summary>
        /// Validates and creates document record permanently after temporary upload process completed.
        /// </summary>
        /// <param name="document">The document.</param>
        public async Task<IActionResult> Create([FromBody] DocumentDTO document)
        {
            //get entity name if this document is uploading to a specific entity.
            string uniqueStandardizedName = document.FileName.Replace(" ", "-");

            uniqueStandardizedName = Regex.Replace(uniqueStandardizedName, @"[^\u0000-\u007F]", string.Empty);

            Document currentDoc = new Document()
            {
                FileSize = document.FileSize,
                Description = document.Description,
                ModuleId = document.ModuleId,
                Name = document.FileName,
                CreatedAt = DateTime.UtcNow,
                Type = document.MimeType,
                UniqueName = document.UniqueFileName,
                RecordId = document.RecordId,
                Deleted = false
            };
            if (await _documentRepository.CreateAsync(currentDoc) != null)
            {
                //transfer file to the permanent storage by committing it.
                await AzureStorage.CommitFile(document.UniqueFileName, currentDoc.UniqueName, currentDoc.Type, string.Format("inst-{0}", AppUser.TenantGuid), document.ChunkSize, _configuration);
                return Ok(currentDoc.Id.ToString());
            }

            return BadRequest("Couldn't Create Document!");
        }

        [Route("image_create"), HttpPost]
        /// <summary>
        /// Validates and creates document record permanently after temporary upload process completed.
        /// </summary>
        /// <param name="document">The document.</param>
        public async Task<IActionResult> ImageCreate([FromBody] JObject data)
        {
            string UniqueFileName, MimeType, TenantId;
            UniqueFileName = data["UniqueFileName"]?.ToString();
            MimeType = data["MimeType"]?.ToString();
            TenantId = data["TenantId"]?.ToString();

            int ChunkSize = Int32.Parse(data["ChunkSize"]?.ToString());

            if (UniqueFileName != null)
            {
                await AzureStorage.CommitFile(UniqueFileName, UniqueFileName, MimeType, "record-detail-" + TenantId, ChunkSize, _configuration);
                return Ok(UniqueFileName);
            }

            return BadRequest("Couldn't Create Document!");
        }

        /// <summary>
        /// Gets last five document records for an entity.
        /// </summary>
        /// <param name="EntityType">Type of the entity.</param>
        /// <param name="EntityID">The entity identifier.</param>
        /// <param name="InstanceID">The instance identifier.</param>
        /// <returns>IList{DTO.DocumentResult}.</returns>
        [Route("GetEntityDocuments"), HttpPost]
        public async Task<IActionResult> GetEntityDocuments([FromBody] DocumentBindingModel req)
        {
            //Get last 5 entity documents and return it to the client.
            var result = await _documentRepository.GetTopEntityDocuments(new DocumentRequest()
            {
                TenantId = req.TenantId,
                ModuleId = req.ModuleId,
                RecordId = req.RecordId,
                UserId = AppUser.Id,
                ContainerId = AppUser.TenantGuid
            });

            if (result.Documents.Count > 0)
            {
                var bucketName = UnifiedStorage.GetPath("attachment", AppUser.TenantId);

                foreach (var document in result.Documents)
                {
                    document.FileUrl = _storage.GetShareLink(bucketName, document.UniqueName, DateTime.UtcNow.AddDays(1));
                }
            }

            return Ok(result);
        }

        /// <summary>
        /// Gets documents for the requested entity and instance. If entity id is empty, query will be resulted with all documents of instance.
        /// </summary>
        /// <param name="EntityType">Type of the entity.</param>
        /// <param name="EntityID">The entity identifier.</param>
        /// <param name="TenantId">The instance identifier.</param>
        /// <returns>DocumentExplorerResult.</returns>
        [Route("GetDocuments"), HttpPost]
        public async Task<IActionResult> GetDocuments([FromBody] DocumentRequest req)
        {
            //validate the instance id by the session instances.

            if (req.TenantId != AppUser.TenantId)
            {
                //if the requested instance does not belong to the current session, than return Forbidden status message
                return Forbid();
            }

            //var docresult = crmDocuments.GetDocuments(req.InstanceID, req.EntityID);

            return Ok(await _documentRepository.GetDocuments(new DocumentRequest()
            {
                TenantId = req.TenantId,
                ModuleId = req.ModuleId,
                RecordId = req.RecordId,
                UserId = AppUser.Id,
                ContainerId = AppUser.TenantGuid
            }));
        }

        /// <summary>
        /// Get document info with id.
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        [Route("GetDocument"), HttpPost]
        public async Task<IActionResult> GetDocumentById([FromBody] int fileID)
        {
            //get the document record from database
            var doc = await _documentRepository.GetById(fileID);
            if (doc != null)
            {
                return Ok(doc);
            }

            return Ok();
        }

        /// <summary>
        /// Downloads the file from cloud to client as a stream.
        /// </summary>
        /// <param name="fileID"></param>
        /// <returns></returns>
        [Route("Download"), HttpGet]
        public async Task<IActionResult> Download([FromQuery(Name = "file_id")] int fileID)
        {
            //get the document record from database
            var doc = await _documentRepository.GetById(fileID);
            string publicName = "";

            if (doc != null)
            {
                //if there is a document with this id, try to get it from blob AzureStorage.
                var blob = AzureStorage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), doc.UniqueName, _configuration);
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


                Response.Headers.Add("Content-Disposition", "attachment; filename=" + publicName); // force download
                await blob.DownloadToStreamAsync(Response.Body);
                return new EmptyResult();
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
        public async Task<IActionResult> DownloadModuleDocument([FromQuery(Name = "module")] string module, [FromQuery(Name = "fileName")] string fileName, [FromQuery(Name = "fileNameExt")] string fileNameExt, [FromQuery(Name = "fieldName")] string fieldName, [FromQuery(Name = "recordId")] string recordId)
        {
            if (!string.IsNullOrEmpty(module) && !string.IsNullOrEmpty(fileNameExt) && !string.IsNullOrEmpty(fieldName))
            {
                var containerName = "module-documents";
                //var ext = Path.GetExtension(fileName);

                module = module.Replace("_", "-");

                string uniqueFileName = $"{AppUser.TenantGuid}/{module}/{recordId}_{fieldName}.{fileNameExt}";
                var docName = fileName + "." + fileNameExt;

                //if there is a document with this id, try to get it from blob AzureStorage.
                var blob = AzureStorage.GetBlob(containerName, uniqueFileName, _configuration);
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
                //return await AzureStorage.DownloadToFileStreamResultAsync(blob, publicName);


                Response.Headers.Add("Content-Disposition", "attachment; filename=" + docName); // force download
                await blob.DownloadToStreamAsync(Response.Body);
                return new EmptyResult();
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
        public async Task<IActionResult> Remove([FromBody] DocumentDTO doc)
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

        /// <summary>
        /// Modifies the file record in database.
        /// </summary>
        /// <param name="document">The document.</param>
        [Route("Modify"), HttpPost]
        public async Task<IActionResult> Modify([FromBody] DocumentDTO document)
        {
            var updatedDoc = await _documentRepository.UpdateAsync(new Document()
            {
                Id = document.ID,
                Description = document.Description,
                ModuleId = document.ModuleId,
                RecordId = document.RecordId,
                Name = document.FileName?.ToString()
            });

            if (updatedDoc != null)
            {
                return Ok(updatedDoc);
            }

            return NotFound();
        }

        [Route("document_search"), HttpPost]
        public IActionResult SearchDocument([FromBody] DocumentFilterRequest filterRequest)
        {
            if (filterRequest != null && filterRequest.Filters.Count > 0)
            {
                DocumentSearch search = new DocumentSearch();

                var searchIndexName = AppUser.TenantGuid + "-" + filterRequest.Module;

                if (filterRequest.Module == null || filterRequest.Top > 50)
                {
                    return BadRequest();
                }

                var results = search.AdvancedSearchDocuments(searchIndexName, filterRequest.Filters, filterRequest.Top, filterRequest.Skip, _configuration);

                return Ok(results);
            }

            return BadRequest();
        }

        [Route("find"), HttpPost]
        public async Task<ICollection<DocumentResult>> Find([FromBody] DocumentFindRequest request)
        {
            var documents = await _documentRepository.GetDocumentsByLimit(request);

            return documents;
        }

        [Route("count"), HttpPost]
        public async Task<int> Count([FromBody] DocumentFindRequest request)
        {
            return await _documentRepository.Count(request);
        }

        public string GetMimeType(string name)
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

        [Route("upload_document_hex"), HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadDocument([FromBody]JObject data)
        {
            StringValues bucketName = UnifiedStorage.GetPath("attachment", AppUser.TenantId);

            string file,
               description,
               moduleName,
               moduleId,
               recordId,
               fileName;
            moduleName = data["module_name"]?.ToString();
            file = data["file"]?.ToString();
            description = data["description"]?.ToString();
            moduleId = data["module_id"]?.ToString();
            recordId = data["record_id"]?.ToString();
            fileName = data["file_name"]?.ToString();

            int parsedModuleId;
            Guid parsedInstanceId = Guid.NewGuid();
            int parsedRecordId;

            if (string.IsNullOrEmpty(file))
                return BadRequest("Please send file hex string.");

            if (string.IsNullOrEmpty(recordId))
                return BadRequest("Please send record_id.");

            if (!int.TryParse(recordId, out parsedRecordId))
                return BadRequest("Please send valid record_id.");

            if (string.IsNullOrEmpty(fileName))
                return BadRequest("Please send file hex string.");

            if (fileName.Split('.').Length != 2)
                return BadRequest("file_name not include special characters and multiple dot. Also dont forget to send file type like test.pdf");

            if (!string.IsNullOrEmpty(moduleId))
            {
                var isNumeric = int.TryParse(moduleId, out parsedModuleId);
                if (!isNumeric)
                    return BadRequest("Please send integer for module_id parameter.");

                var module = await _moduleRepository.GetById(parsedModuleId);

                if (module == null)
                    return BadRequest("Module not found.");
            }
            else if (!string.IsNullOrEmpty(moduleName))
            {
                var module = await _moduleRepository.GetByNameBasic(moduleName);

                if (module == null)
                    return BadRequest("Module not found.");

                parsedModuleId = module.Id;
            }
            else
                return BadRequest("Please send module_id or module_name paratemer.");


            var ext = Path.GetExtension(fileName);
            var uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;
            Byte[] fileByte = Convert.FromBase64String(file);

            using (Stream stream = new MemoryStream(fileByte))
            {
                await _storage.Upload(bucketName, uniqueName, stream);
            }

            var result = new DocumentUploadResult
            {
                ContentType = UnifiedStorage.GetMimeType(ext),
                UniqueName = uniqueName,
                Chunks = 0
            };

            Document currentDoc = new Document()
            {
                FileSize = fileByte.Length,
                Description = description,
                ModuleId = parsedModuleId,
                Name = fileName,
                CreatedAt = DateTime.UtcNow,
                Type = UnifiedStorage.GetMimeType(ext),
                UniqueName = uniqueName,
                RecordId = parsedRecordId,
                Deleted = false
            };

            if (await _documentRepository.CreateAsync(currentDoc) != null)
            {
                return Ok(currentDoc.Id.ToString());
            }

            //return content type of the file to the client
            return Ok(result);

        }

        [Route("document_url"), HttpGet]
        public async Task<string> DocumentUrl([FromQuery(Name = "fileId")]int fileId)
        {
            var doc = await _documentRepository.GetById(fileId);
            if (doc != null)
            {
                StringValues bucketName = UnifiedStorage.GetPath("attachment", AppUser.TenantId);

                GetPreSignedUrlRequest request = new GetPreSignedUrlRequest();
                request.BucketName = bucketName;
                request.Key = doc.UniqueName;
                request.Expires = DateTime.UtcNow.AddYears(100);
                request.Protocol = Protocol.HTTP;
                string url = _storage.GetDocUrl(request);

                return url;
            }
            else
            {
                //there is no such file, return
                throw new Exception("Document does not exist in the storage!");
            }
        }
    }
}