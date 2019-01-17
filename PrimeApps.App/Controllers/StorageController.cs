using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Document;
using Microsoft.WindowsAzure.Storage.Blob;
using PrimeApps.App.Storage;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using PrimeApps.App.Storage.Unified;
using Amazon.S3.Model;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Protocols;
using PrimeApps.Model;
using System.Text.RegularExpressions;
using Document = PrimeApps.Model.Entities.Tenant.Document;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Collections.Generic;
using static PrimeApps.App.Storage.UnifiedStorage;

namespace PrimeApps.App.Controllers
{
    [Route("api/storage"), Authorize]
    public class StorageController : ApiBaseController
    {
        private IDocumentRepository _documentRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private ITemplateRepository _templateRepository;
        private INoteRepository _noteRepository;
        private ISettingRepository _settingRepository;
        private IUnifiedStorage _storage;
        private IConfiguration _configuration;


        public StorageController(IDocumentRepository documentRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, ITemplateRepository templateRepository, INoteRepository noteRepository, IPicklistRepository picklistRepository, ISettingRepository settingRepository, IUnifiedStorage storage, IConfiguration configuration)
        {
            _documentRepository = documentRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _templateRepository = templateRepository;
            _noteRepository = noteRepository;
            _settingRepository = settingRepository;
            _storage = storage;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_documentRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_templateRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_noteRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_settingRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
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
                chunksStr, uploadId, chunkStr, fileName, responseList, type;
            form.TryGetValue("chunks", out chunksStr);
            form.TryGetValue("chunk", out chunkStr);
            form.TryGetValue("name", out fileName);
            form.TryGetValue("upload_id", out uploadId);
            form.TryGetValue("response_list", out responseList);
            form.TryGetValue("type", out type);
            ObjectType objectType = UnifiedStorage.GetType(type);


            bucketName = GetPath(type, AppUser.TenantId);


            int chunk = 0,
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
                    if (objectType == ObjectType.ATTACHMENT)
                    {
                        response.PublicURL = _storage.GetShareLink(bucketName, fileName, DateTime.UtcNow.AddMonths(3), Amazon.S3.Protocol.HTTP);
                    }
                }
            }
            catch (Exception ex)
            {
                await _storage.AbortMultipartUpload(bucketName, fileName, uploadId);
                response.Status = MultipartStatusEnum.Aborted;
            }

            return Json(response);
        }

        [HttpPost("upload_whole")]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadWhole()
        {
            var parser = new HttpMultipartParser(Request.Body, "file");
            StringValues bucketName = $"tenant{AppUser.TenantId}";

            //if it is successfully parsed continue.
            if (parser.Success)
            {
                if (parser.FileContents.Length <= 0)
                {
                    //check the file size if it is 0 bytes then return client with that error code.
                    return BadRequest();
                }

                var ext = Path.GetExtension(parser.Filename);
                var uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;

                await _storage.Upload(bucketName, uniqueName, Request.Body);

                var result = new DocumentUploadResult
                {
                    ContentType = parser.ContentType,
                    UniqueName = uniqueName,
                    Chunks = 0
                };

                //return content type of the file to the client
                return Ok(result);
            }

            //this request invalid because there is no file, return fail code to the client.
            return NotFound();
        }


        [Route("upload_hex"), HttpPost]
        [DisableRequestSizeLimit]
        public async Task<IActionResult> UploadHex([FromBody]JObject data)
        {
            string instanceId,
              file,
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
            instanceId = data["instance_id"]?.ToString();

            int parsedModuleId;
            Guid parsedInstanceId = Guid.NewGuid();
            int parsedRecordId;
            byte[] fileBytes;

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

            if (string.IsNullOrEmpty(instanceId))
                return BadRequest("Please send instance_id.");

            if (!string.IsNullOrEmpty(instanceId))
                if (!Guid.TryParse(instanceId, out parsedInstanceId))
                    return BadRequest("Please send valid instance_id.");

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

            try
            {
                fileBytes = Enumerable.Range(0, file.Length)
                     .Where(x => x % 2 == 0)
                     .Select(x => Convert.ToByte(file.Substring(x, 2), 16))
                     .ToArray();
            }
            catch (Exception ex)
            {
                return BadRequest("Hex string is not valid. Exception is :" + ex.Message);
            }

            var ext = Path.GetExtension(fileName);
            var uniqueName = Guid.NewGuid().ToString().Replace("-", "") + ext;

            string uniqueStandardizedName = fileName.Replace(" ", "-");

            uniqueStandardizedName = Regex.Replace(uniqueStandardizedName, @"[^\u0000-\u007F]", string.Empty);

            MemoryStream bytesToStream = new MemoryStream(fileBytes);
            string bucketPath = UnifiedStorage.GetPath("", AppUser.TenantId);
            await _storage.Upload(bucketPath, uniqueName, bytesToStream);

            Document currentDoc = new Document()
            {
                FileSize = fileBytes.Length,
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

            return BadRequest("Couldn't Create Document!");

            //this request invalid because there is no file, return fail code to the client.
            //return NotFound();
        }
        [Route("create"), HttpPost]
        /// <summary>
        /// Validates and creates document record permanently after temporary upload process completed.
        /// </summary>
        /// <param name="document">The document.</param>
        public async Task<IActionResult> Create([FromBody]DocumentDTO document)
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
                return Ok(currentDoc.Id.ToString());
            }

            return BadRequest("Couldn't Create Document!");

        }



    }
}
