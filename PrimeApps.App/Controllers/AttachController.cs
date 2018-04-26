using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrimeApps.App.Cache;
using PrimeApps.App.Helpers;
using PrimeApps.App.Results;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Repositories;

namespace PrimeApps.App.Controllers
{
    [Route("attach"), Authorize]
    public class AttachController : Controller
    {
        [Route("download"), HttpGet]
        public async Task<FileStreamResult> Download(int FileId)
        {

            var userId = await ApplicationUser.GetId(User.Identity.Name);

            PlatformUser user;
            using (var _platformUserRepository = new PlatformUserRepository(new PlatformDBContext()))
            {
                user = await _platformUserRepository.GetWithTenant(userId);
            }

            using (var tenantDbContext = new TenantDBContext(user.TenantId.Value))
            {
                var publicName = "";
                var doc = await tenantDbContext.Documents.FirstOrDefaultAsync(x => x.Id == FileId && x.Deleted == false);
                if (doc != null)
                {
                    //if there is a document with this id, try to get it from blob storage.
                    var blob = Storage.GetBlob(string.Format("inst-{0}", user.Tenant.GuidId), doc.UniqueName);
                    try
                    {
                        //try to get the attributes of blob.
                        await blob.FetchAttributesAsync();
                    }
                    catch (Exception)
                    {
                        //if there is an exception, it means there is no such file.
                        throw new Exception("Something happened");
                    }

                    //Is bandwidth enough to download this file?
                    //Bandwidth is enough, send the Storage.
                    publicName = doc.Name;


                    /*var file = new FileDownloadResult()
                    {
                        Blob = blob,
                        PublicName = doc.Name
                    };*/
                    Stream rtn = null;
                    var aRequest = (HttpWebRequest)WebRequest.Create(blob.Uri.AbsoluteUri);
                    var aResponse = (HttpWebResponse)aRequest.GetResponse();
                    rtn = aResponse.GetResponseStream();
                    return File(rtn, DocumentHelper.GetType(publicName), publicName);
                }
                else
                {
                    //there is no such file, return
                    throw new Exception("Something happened");
                }
            }
        }
    }
}