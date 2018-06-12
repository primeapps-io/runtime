using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using PrimeApps.App.Helpers;
using PrimeApps.App.Storage;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using OfisimCRM.Model.Repositories.Interfaces;
using OfisimCRM.DTO.Record;
using Aspose.Cells;
using System.Data;
using System.Web;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Aspose.Words.Saving;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Repositories;

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

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);

            base.OnActionExecuting(context);
        }

        [Route("download"), HttpGet]
        public async Task<FileStreamResult> Download([FromQuery(Name = "FileId")] int FileId)
        {
            var tenant = await _tenantRepository.GetAsync(AppUser.TenantId);

            using (var tenantDbContext = new TenantDBContext(AppUser.TenantId))
            {
                var publicName = "";
                var doc = await tenantDbContext.Documents.FirstOrDefaultAsync(x =>
                    x.Id == FileId && x.Deleted == false);
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
                    var aRequest = (HttpWebRequest) WebRequest.Create(blob.Uri.AbsoluteUri);
                    var aResponse = (HttpWebResponse) aRequest.GetResponse();
                    rtn = aResponse.GetResponseStream();
                    return File(rtn, DocumentHelper.GetType(publicName), publicName);
                }
                else
                {
                    //there is no such file, return
                    throw new Exception("Something happened");
                }
            }


            public async Task<FileStreamResult> ExportExcel(string module, string locale = "", bool? normalize = false,
                int? timezoneOffset = 180)
            {
                if (string.IsNullOrWhiteSpace(module))
                    throw new HttpException(400, "Module field is required");

                using (var dbContext = new ApplicationDbContext())
                {
                    var userId = AsyncHelpers.RunSync(() => Cache.ApplicationUser.Get(User.Identity.Name));
                    var user = crmUser.GetById(userId);
                    using (var tenantDbContext = new TenantDBContext(user.TenantID))
                    {
                        using (var moduleRepository = new ModuleRepository(tenantDbContext))
                        {
                            using (var recordRepository = new RecordRepository(tenantDbContext))
                            {
                                var moduleEntity = await moduleRepository.GetByName(module);
                                var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
                                var nameModule = user.culture.Contains("tr")
                                    ? moduleEntity.LabelTrPlural
                                    : moduleEntity.LabelEnPlural;
                                byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(nameModule);
                                var moduleName = System.Text.Encoding.ASCII.GetString(bytes);
                                Workbook workbook = new Workbook(FileFormatType.Xlsx);
                                Worksheet worksheetData = workbook.Worksheets[0];
                                worksheetData.Name = "Data";
                                DataTable dt = new DataTable("Excel");
                                Worksheet worksheet2 = workbook.Worksheets.Add("Report Formula");
                                var lookupModules =
                                    await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, moduleRepository);

                                var findRequest = new FindRequest();
                                findRequest.Fields = new List<string>();

                                for (int i = 0; i < fields.Count; i++)
                                {
                                    var field = fields[i];

                                    if (field.DataType != Model.Enums.DataType.Lookup)
                                    {
                                        findRequest.Fields.Add(field.Name);
                                    }
                                    else
                                    {
                                        var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
                                        var primaryField = lookupModule.Fields.Single(x => x.Primary);
                                        findRequest.Fields.Add(
                                            field.Name + "." + field.LookupType + "." + primaryField.Name);
                                    }
                                }

                                var records = recordRepository.Find(moduleEntity.Name, findRequest);

                                for (int i = 0; i < fields.Count; i++)
                                {
                                    var field = fields[i];
                                    dt.Columns.Add(field.LabelTr.ToString());
                                }

                                for (int j = 0; j < records.Count; j++)
                                {
                                    var record = records[j];
                                    var dr = dt.NewRow();

                                    for (int i = 0; i < fields.Count; i++)
                                    {
                                        var field = fields[i];

                                        if (field.DataType != Model.Enums.DataType.Lookup)
                                        {
                                            dr[i] = record[field.Name];
                                        }
                                        else
                                        {
                                            var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
                                            var primaryField = lookupModule.Fields.Single(x => x.Primary);
                                            dr[i] = record[
                                                field.Name + "." + field.LookupType + "." + primaryField.Name];
                                        }
                                    }

                                    dt.Rows.Add(dr);
                                }

                                worksheetData.Cells.ImportDataTable(dt, true, "A1");

                                workbook.Save(System.Web.HttpContext.Current.Response, moduleName + ".xlsx",
                                    ContentDisposition.Attachment, new OoxmlSaveOptions());
                                Response.End();

                                return null;
                            }
                        }
                    }
                }
            }

            public async Task<FileStreamResult> ExportExcelView(string module, string locale = "",
                bool? normalize = false, int? timezoneOffset = 180)
            {
                if (string.IsNullOrWhiteSpace(module))
                    throw new HttpException(400, "Module field is required");

                using (var dbContext = new ApplicationDbContext())
                {
                    var userId = AsyncHelpers.RunSync(() => Cache.ApplicationUser.Get(User.Identity.Name));
                    var user = crmUser.GetById(userId);
                    using (var tenantDbContext = new TenantDBContext(user.TenantID))
                    {
                        using (var moduleRepository = new ModuleRepository(tenantDbContext))
                        {
                            using (var recordRepository = new RecordRepository(tenantDbContext))
                            {
                                var moduleEntity = await moduleRepository.GetByName(module);
                                var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
                                var nameModule = user.culture.Contains("tr")
                                    ? moduleEntity.LabelTrPlural
                                    : moduleEntity.LabelEnPlural;
                                byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(nameModule);
                                var moduleName = System.Text.Encoding.ASCII.GetString(bytes);
                                Workbook workbook = new Workbook(FileFormatType.Xlsx);
                                Worksheet worksheetData = workbook.Worksheets[0];
                                worksheetData.Name = "Data";
                                DataTable dt = new DataTable("Excel");
                                var lookupModules =
                                    await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, moduleRepository);

                                var findRequest = new FindRequest();
                                findRequest.Fields = new List<string>();

                                for (int i = 0; i < fields.Count; i++)
                                {
                                    var field = fields[i];

                                    if (field.DataType != Model.Enums.DataType.Lookup)
                                    {
                                        findRequest.Fields.Add(field.Name);
                                    }
                                    else
                                    {
                                        var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
                                        var primaryField = lookupModule.Fields.Single(x => x.Primary);
                                        findRequest.Fields.Add(
                                            field.Name + "." + field.LookupType + "." + primaryField.Name);
                                    }
                                }

                                var records = recordRepository.Find(moduleEntity.Name, findRequest);

                                for (int i = 0; i < fields.Count; i++)
                                {
                                    var field = fields[i];
                                    dt.Columns.Add(field.LabelTr.ToString());
                                }

                                for (int j = 0; j < records.Count; j++)
                                {
                                    var record = records[j];
                                    var dr = dt.NewRow();

                                    for (int i = 0; i < fields.Count; i++)
                                    {
                                        var field = fields[i];

                                        if (field.DataType != Model.Enums.DataType.Lookup)
                                        {
                                            dr[i] = record[field.Name];
                                        }
                                        else
                                        {
                                            var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
                                            var primaryField = lookupModule.Fields.Single(x => x.Primary);
                                            dr[i] = record[
                                                field.Name + "." + field.LookupType + "." + primaryField.Name];
                                        }
                                    }

                                    dt.Rows.Add(dr);
                                }

                                worksheetData.Cells.ImportDataTable(dt, true, "A1");

                                workbook.Save(System.Web.HttpContext.Current.Response, moduleName + ".xlsx",
                                    ContentDisposition.Attachment, new OoxmlSaveOptions());
                                Response.End();

                                return null;
                            }
                        }
                    }
                }
            }

            public async Task<FileStreamResult> ExportExcelNoData(string module, int templateId, string templateName,
                string locale = "", bool? normalize = false, int? timezoneOffset = 180)
            {
                if (string.IsNullOrWhiteSpace(module) || templateId == null || templateId == 0)
                    throw new HttpException(400, "Module field is required");

                using (var dbContext = new ApplicationDbContext())
                {
                    var userId = AsyncHelpers.RunSync(() => Cache.ApplicationUser.Get(User.Identity.Name));
                    var user = crmUser.GetById(userId);

                    using (var tenantDbContext = new TenantDBContext(user.TenantID))
                    {
                        using (var moduleRepository = new ModuleRepository(tenantDbContext))
                        {
                            using (var templateRepostory = new TemplateRepository(tenantDbContext))
                            {
                                using (var recordRepository = new RecordRepository(tenantDbContext))
                                {
                                    var moduleEntity = await moduleRepository.GetByName(module);
                                    var Module = await moduleRepository.GetByName(module);
                                    var template = await templateRepostory.GetById(templateId);
                                    var blob = Storage.GetBlob(string.Format("inst-{0}", user.defaultInstanceID),
                                        $"templates/{template.Content}");
                                    var fields = Module.Fields.OrderBy(x => x.Id).ToList();
                                    var tempsName = templateName;
                                    byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(tempsName);
                                    var tempName = System.Text.Encoding.ASCII.GetString(bytes);
                                    var lookupModules =
                                        await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity,
                                            moduleRepository);

                                    var findRequest = new FindRequest();
                                    findRequest.Fields = new List<string>();

                                    for (int i = 0; i < fields.Count; i++)
                                    {
                                        var field = fields[i];

                                        if (field.DataType != Model.Enums.DataType.Lookup)
                                        {
                                            findRequest.Fields.Add(field.Name);
                                        }
                                        else
                                        {
                                            var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
                                            var primaryField = lookupModule.Fields.Single(x => x.Primary);
                                            findRequest.Fields.Add(
                                                field.Name + "." + field.LookupType + "." + primaryField.Name);
                                        }
                                    }

                                    var records = recordRepository.Find(moduleEntity.Name, findRequest);

                                    using (var temp = new MemoryStream())
                                    {
                                        await blob.DownloadToStreamAsync(temp);
                                        Workbook workbook = new Workbook(temp);
                                        Worksheet worksheetReportAdd = workbook.Worksheets.Add("Report");
                                        Worksheet worksheetData = workbook.Worksheets[0];
                                        Worksheet worksheetReportFormul = workbook.Worksheets[1];
                                        Worksheet worksheetReport = workbook.Worksheets["Report"];
                                        var row = worksheetReportFormul.Cells.MaxDisplayRange.RowCount + 1;
                                        var col = worksheetReportFormul.Cells.MaxDisplayRange.ColumnCount + 1;
                                        var count = records.Count;

                                        worksheetData.Cells.DeleteRows(0, count + 1);

                                        DataTable dt = new DataTable("Excel");

                                        for (int i = 0; i < fields.Count; i++)
                                        {
                                            var field = fields[i];
                                            dt.Columns.Add(field.LabelTr.ToString());
                                        }

                                        for (int j = 0; j < records.Count; j++)
                                        {
                                            var record = records[j];
                                            var dr = dt.NewRow();

                                            for (int i = 0; i < fields.Count; i++)
                                            {
                                                var field = fields[i];

                                                if (field.DataType != Model.Enums.DataType.Lookup)
                                                {
                                                    dr[i] = record[field.Name];
                                                }
                                                else
                                                {
                                                    var lookupModule =
                                                        lookupModules.Single(x => x.Name == field.LookupType);
                                                    var primaryField = lookupModule.Fields.Single(x => x.Primary);
                                                    dr[i] = record[
                                                        field.Name + "." + field.LookupType + "." + primaryField.Name];
                                                }
                                            }

                                            dt.Rows.Add(dr);
                                        }

                                        worksheetData.Cells.ImportDataTable(dt, true, "A1");
                                        workbook.CalculateFormula();
                                        if (row > 0 && col > 0)
                                        {
                                            var fromRange = worksheetReportFormul.Cells.CreateRange(0, 0, row, col);
                                            var toRange = worksheetReport.Cells.CreateRange(0, 0, 1, 1);
                                            toRange.CopyValue(fromRange);
                                        }

                                        workbook.Worksheets.RemoveAt("Data");
                                        workbook.Worksheets.RemoveAt("Report Formula");
                                        workbook.Worksheets.RemoveAt("Evaluation Warning");

                                        workbook.Save(System.Web.HttpContext.Current.Response, tempName + ".xlsx",
                                            ContentDisposition.Attachment, new OoxmlSaveOptions());
                                        Response.End();

                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            public async Task<FileStreamResult> ExportExcelData(string module, string templateName, int templateId,
                string locale = "", bool? normalize = false, int? timezoneOffset = 180)
            {
                if (string.IsNullOrWhiteSpace(module) || templateId == null || templateId == 0)
                    throw new HttpException(400, "Module field is required");

                using (var dbContext = new ApplicationDbContext())
                {
                    var userId = AsyncHelpers.RunSync(() => Cache.ApplicationUser.Get(User.Identity.Name));
                    var user = crmUser.GetById(userId);

                    using (var tenantDbContext = new TenantDBContext(user.TenantID))
                    {
                        using (var moduleRepository = new ModuleRepository(tenantDbContext))
                        {
                            using (var templateRepostory = new TemplateRepository(tenantDbContext))
                            {
                                using (var recordRepository = new RecordRepository(tenantDbContext))
                                {
                                    var moduleEntity = await moduleRepository.GetByName(module);
                                    var template = await templateRepostory.GetById(templateId);
                                    var blob = Storage.GetBlob(string.Format("inst-{0}", user.defaultInstanceID),
                                        $"templates/{template.Content}");
                                    var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
                                    var tempsName = templateName;
                                    byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(tempsName);
                                    var tempName = Encoding.ASCII.GetString(bytes);
                                    var lookupModules =
                                        await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity,
                                            moduleRepository);

                                    var findRequest = new FindRequest();
                                    findRequest.Fields = new List<string>();

                                    for (int i = 0; i < fields.Count; i++)
                                    {
                                        var field = fields[i];

                                        if (field.DataType != Model.Enums.DataType.Lookup)
                                        {
                                            findRequest.Fields.Add(field.Name);
                                        }
                                        else
                                        {
                                            var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
                                            var primaryField = lookupModule.Fields.Single(x => x.Primary);
                                            findRequest.Fields.Add(
                                                field.Name + "." + field.LookupType + "." + primaryField.Name);
                                        }
                                    }

                                    var records = recordRepository.Find(moduleEntity.Name, findRequest);

                                    using (var temp = new MemoryStream())
                                    {
                                        await blob.DownloadToStreamAsync(temp);
                                        Workbook workbook = new Workbook(temp);
                                        Worksheet worksheetReportAdd = workbook.Worksheets.Add("Report");
                                        Worksheet worksheetData = workbook.Worksheets[0];
                                        Worksheet worksheetReportFormul = workbook.Worksheets[1];
                                        Worksheet worksheetReport = workbook.Worksheets["Report"];
                                        var row = worksheetReportFormul.Cells.MaxDisplayRange.RowCount + 1;
                                        var col = worksheetReportFormul.Cells.MaxDisplayRange.ColumnCount + 1;
                                        var count = records.Count;

                                        worksheetData.Cells.DeleteRows(0, count + 1);

                                        DataTable dt = new DataTable("Excel");

                                        for (int i = 0; i < fields.Count; i++)
                                        {
                                            var field = fields[i];
                                            dt.Columns.Add(field.LabelTr.ToString());
                                        }

                                        for (int j = 0; j < records.Count; j++)
                                        {
                                            var record = records[j];
                                            var dr = dt.NewRow();

                                            for (int i = 0; i < fields.Count; i++)
                                            {
                                                var field = fields[i];

                                                if (field.DataType != Model.Enums.DataType.Lookup)
                                                {
                                                    dr[i] = record[field.Name];
                                                }
                                                else
                                                {
                                                    var lookupModule =
                                                        lookupModules.Single(x => x.Name == field.LookupType);
                                                    var primaryField = lookupModule.Fields.Single(x => x.Primary);
                                                    dr[i] = record[
                                                        field.Name + "." + field.LookupType + "." + primaryField.Name];
                                                }
                                            }

                                            dt.Rows.Add(dr);
                                        }

                                        worksheetData.Cells.ImportDataTable(dt, true, "A1");
                                        workbook.CalculateFormula();
                                        if (row > 0 && col > 0)
                                        {
                                            var fromRange = worksheetReportFormul.Cells.CreateRange(0, 0, row, col);
                                            var toRange = worksheetReport.Cells.CreateRange(0, 0, 1, 1);
                                            toRange.CopyValue(fromRange);
                                        }

                                        workbook.Worksheets.RemoveAt("Evaluation Warning");

                                        workbook.Save(System.Web.HttpContext.Current.Response, tempName + ".xlsx",
                                            ContentDisposition.Attachment, new OoxmlSaveOptions());
                                        Response.End();

                                        return null;
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}