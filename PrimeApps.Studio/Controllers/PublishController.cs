using System;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common.Profile;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Helpers;
using PrimeApps.Studio.Models;
using PrimeApps.Studio.Services;
using PrimeApps.Util.Storage;

namespace PrimeApps.Studio.Controllers
{
    [Route("api/publish")]
    public class PublishController : DraftBaseController
    {
        private IBackgroundTaskQueue _queue;
        private IReleaseRepository _releaseRepository;
        private IAppDraftRepository _appDraftRepository;
        private IHistoryDatabaseRepository _historyDatabaseRepository;
        private IHistoryStorageRepository _historyStorageRepository;
        private IPermissionHelper _permissionHelper;
        private IUnifiedStorage _storage;
        private IReleaseHelper _publishHelper;

        public PublishController(IReleaseHelper publishHelper,
            IReleaseRepository releaseRepository,
            IAppDraftRepository appDraftRepository,
            IHistoryDatabaseRepository historyDatabaseRepository,
            IHistoryStorageRepository historyStorageRepository,
            IPermissionHelper permissionHelper,
            IUnifiedStorage storage,
            IBackgroundTaskQueue queue)
        {
            _publishHelper = publishHelper;
            _releaseRepository = releaseRepository;
            _historyDatabaseRepository = historyDatabaseRepository;
            _historyStorageRepository = historyStorageRepository;
            _appDraftRepository = appDraftRepository;
            _permissionHelper = permissionHelper;
            _storage = storage;
            _queue = queue;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_releaseRepository);
            SetCurrentUser(_historyDatabaseRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_historyStorageRepository, PreviewMode, TenantId, AppId);
            base.OnActionExecuting(context);
        }

        [HttpGet]
        [Route("get_active_process")]
        public async Task<IActionResult> GetActiveProcess()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "publish", RequestTypeEnum.Create))
                return StatusCode(403);

            var process = await _releaseRepository.GetActiveProcess((int)AppId);

            return Ok(process);
        }

        [HttpGet]
        [Route("get_last_deployment")]
        public async Task<IActionResult> GetLastDeployment()
        {
            if (!_permissionHelper.CheckUserProfile(UserProfile, "publish", RequestTypeEnum.Create))
                return StatusCode(403);

            var result = await _releaseRepository.GetLastRelease((int)AppId);

            return Ok(result);
        }

        /*[HttpPost]
        [Route("create")]
        public async Task<IActionResult> Create([FromBody]JObject model)
        {
            
        }*/

        [HttpGet]
        [Route("download_package/{id}")]
        [Obsolete]
        public async Task DownloadPackage(int id)
        {
            try
            {
                var bucketName = UnifiedStorage.GetPath("releases", PreviewMode, PreviewMode == "tenant" ? (int)TenantId : (int)AppId);

                var withouts = new string[] {"releases"};
                await _storage.CopyBucket($"app{AppId}", bucketName + "/" + id + "/files", withouts);

                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | SecurityProtocolType.Tls | SecurityProtocolType.Tls | SecurityProtocolType.Tls;

                var stMarged = new System.IO.MemoryStream();
                //Doc.Save(stMarged);


                stMarged.Position = 0;
                var list = await _storage.GetListObject(bucketName + "/" + id + "/");

                /*using (MemoryStream zipStream = new MemoryStream())
                {
                    using (ZipArchive zip = new ZipArchive(zipStream))
                    {
                        foreach (var item in list.S3Objects)
                        {
                            ZipArchiveEntry entry = zip.CreateEntry(item.Key);
                            using (Stream entryStream = entry.Open())
                            {
                                stMarged.CopyTo(entryStream);
                            }
                        }
                    }
                    zipStream.Position = 0;
                }*/

                var token = new CancellationToken();

                foreach (var objt in list.S3Objects)
                {
                    var request1 = new GetObjectRequest {BucketName = objt.BucketName, Key = objt.Key};

                    using (var response = await _storage.Client.GetObjectAsync(request1, token))
                    {
                        await response.WriteResponseStreamToFileAsync(_storage.GetDownloadFolderPath() + "\\" + objt.Key, true, token);
                    }
                }
            }
            catch (Exception ex)
            {
            }
        }

        /*public static void Hloo()
        {
            string startPath = @"c:\example\start";
            string zipPath = @"c:\example\result.zip";
            string extractPath = @"c:\example\extract";

            ZipFile.CreateFromDirectory(startPath, zipPath, CompressionLevel.Fastest, true);

            ZipFile.ExtractToDirectory(zipPath, extractPath);
        }

        public void ConvertToZip(string directoryToZip, string zipFileName)
        {
            try
            {
                //var sourDir = new S3DirectoryInfo(client, bucket, directoryToZip);

                //var destDir = new S3DirectoryInfo(client, bucket, CCUrlHelper.BackupRootFolderPhysicalPath);

                var zipStream = new MemoryStream();
                using (var zip = new ZipArchive(zipStream, ZipArchiveMode.Create, true))
                {
                    zip.CreateEntry(sourDir.FullName);
                    zip.ExtractToDirectory("C:\\");
                }
            }
            catch (Exception ex)
            {
            }
        }*/
    }
}