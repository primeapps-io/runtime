using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrimeApps.App.Helpers;
using PrimeApps.App.Storage;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.App.Controllers
{
    [Route("attach")]
    public class AttachController : BaseController
    {
        private ITenantRepository _tenantRepository;

        public AttachController(ITenantRepository tenantRepository)
        {
            _tenantRepository = tenantRepository;
        }

        [Route("download"), HttpGet]
        public async Task<FileStreamResult> Download(int FileId)
        {
            var tenant = await _tenantRepository.GetAsync(AppUser.TenantId);

            using (var tenantDbContext = new TenantDBContext(AppUser.TenantId))
            {
                var publicName = "";
                var doc = await tenantDbContext.Documents.FirstOrDefaultAsync(x => x.Id == FileId && x.Deleted == false);
                if (doc != null)
                {
                    //if there is a document with this id, try to get it from blob AzureStorage.
                    var blob = AzureStorage.GetBlob(string.Format("inst-{0}", tenant.GuidId), doc.UniqueName);
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
                    //Bandwidth is enough, send the AzureStorage.
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