using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Repositories;
using Aspose.Cells;
using Newtonsoft.Json.Linq;
using System.Net.Mime;

namespace PrimeApps.App.Controllers
{
    [Route("attach")]
    public class AttachController : MvcBaseController
    {
        private ITenantRepository _tenantRepository;
        private IModuleRepository _modulepository;
        private IRecordRepository _recordpository;

        public AttachController(ITenantRepository tenantRepository, IModuleRepository moduleRepository, IRecordRepository recordRepository)
        {
            _tenantRepository = tenantRepository;
            _modulepository = moduleRepository;
            _recordpository = recordRepository;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_modulepository);
            SetCurrentUser(_recordpository);
            base.OnActionExecuting(context);
        }

        //[Route("download")]
        //public async Task<FileStreamResult> Download([FromQuery(Name = "FileId")] int FileId)
        //{

        //    //var tenant = await _tenantRepository.GetAsync(AppUser.TenantId);

        //    using (var tenantDbContext = new TenantDBContext(AppUser.TenantId))
        //    {
        //        var publicName = "";
        //        var doc = await tenantDbContext.Documents.FirstOrDefaultAsync(x =>
        //            x.Id == FileId && x.Deleted == false);
        //        if (doc != null)
        //        {
        //            //if there is a document with this id, try to get it from blob AzureStorage.
        //            var blob = AzureStorage.GetBlob(string.Format("inst-{0}", tenant.GuidId), doc.UniqueName);
        //            try
        //            {
        //                //try to get the attributes of blob.
        //                await blob.FetchAttributesAsync();
        //            }
        //            catch (Exception)
        //            {
        //                //if there is an exception, it means there is no such file.
        //                throw new Exception("Something happened");
        //            }

        //            //Is bandwidth enough to download this file?
        //            //Bandwidth is enough, send the AzureStorage.
        //            publicName = doc.Name;


        //            /*var file = new FileDownloadResult()
        //            {
        //                Blob = blob,
        //                PublicName = doc.Name
        //            };*/
        //            Stream rtn = null;
        //            var aRequest = (HttpWebRequest)WebRequest.Create(blob.Uri.AbsoluteUri);
        //            var aResponse = (HttpWebResponse)aRequest.GetResponse();
        //            rtn = aResponse.GetResponseStream();
        //            return File(rtn, DocumentHelper.GetType(publicName), publicName);
        //        }
        //        else
        //        {
        //            //there is no such file, return
        //            throw new Exception("Something happened");
        //        }
        //    }

        //}

        [Route("export_excel")]
        public async Task<ActionResult> ExportExcel([FromQuery(Name = "module")]string module)
        {
            if (string.IsNullOrWhiteSpace(module))
                throw new HttpRequestException("Module field is required");

            var moduleEntity = await _modulepository.GetByName(module);
            var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
            var nameModule = AppUser.Culture.Contains("tr") ? moduleEntity.LabelTrPlural : moduleEntity.LabelEnPlural;
            //byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(nameModule);
            //var moduleName = System.Text.Encoding.ASCII.GetString(bytes);
            Workbook workbook = new Workbook(FileFormatType.Xlsx);
            Worksheet worksheetData = workbook.Worksheets[0];
            worksheetData.Name = "Data";
            DataTable dt = new DataTable("Excel");
            Worksheet worksheet2 = workbook.Worksheets.Add("Report Formula");
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _modulepository);

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
                    findRequest.Fields.Add(field.Name + "." + field.LookupType + "." + primaryField.Name);
                }
            }

            var records = _recordpository.Find(moduleEntity.Name, findRequest);

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
                        dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];
                    }
                }
                dt.Rows.Add(dr);
            }

            worksheetData.Cells.ImportDataTable(dt, true, "A1");

            workbook.Save(nameModule + ".xlsx", new OoxmlSaveOptions());

            return Ok();
        }

        [Route("export_excel_view")]
        public async Task<ActionResult> ExportExcelView([FromQuery(Name = "module")]string module, string locale = "", bool? normalize = false, int? timezoneOffset = 180)
        {
            if (string.IsNullOrWhiteSpace(module))
                throw new HttpRequestException("Module field is required");

            var moduleEntity = await _modulepository.GetByName(module);
            var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
            var nameModule = AppUser.Culture.Contains("tr") ? moduleEntity.LabelTrPlural : moduleEntity.LabelEnPlural;
            //byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(nameModule);
            //var moduleName = System.Text.Encoding.ASCII.GetString(bytes);
            Workbook workbook = new Workbook(FileFormatType.Xlsx);
            Worksheet worksheetData = workbook.Worksheets[0];
            worksheetData.Name = "Data";
            DataTable dt = new DataTable("Excel");
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _modulepository);

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
                    findRequest.Fields.Add(field.Name + "." + field.LookupType + "." + primaryField.Name);
                }
            }

            var records = _recordpository.Find(moduleEntity.Name, findRequest);

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
                        dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];
                    }
                }
                dt.Rows.Add(dr);
            }

            worksheetData.Cells.ImportDataTable(dt, true, "A1");

            workbook.Save(nameModule + ".xlsx",new OoxmlSaveOptions());
         
            return Ok();
        }

        //public async Task<FileStreamResult> ExportExcelNoData(string module, int templateId, string templateName, string locale = "", bool? normalize = false, int? timezoneOffset = 180)
        //{
        //    if (string.IsNullOrWhiteSpace(module) || templateId == null || templateId == 0)
        //        throw new HttpException(400, "Module field is required");

        //    using (var dbContext = new ApplicationDbContext())
        //    {
        //        var userId = AsyncHelpers.RunSync(() => Cache.ApplicationUser.Get(User.Identity.Name));
        //        var user = crmUser.GetById(userId);

        //        using (var tenantDbContext = new TenantDBContext(user.TenantID))
        //        {
        //            using (var moduleRepository = new ModuleRepository(tenantDbContext))
        //            {
        //                using (var templateRepostory = new TemplateRepository(tenantDbContext))
        //                {
        //                    using (var recordRepository = new RecordRepository(tenantDbContext))
        //                    {
        //                        var moduleEntity = await moduleRepository.GetByName(module);
        //                        var Module = await moduleRepository.GetByName(module);
        //                        var template = await templateRepostory.GetById(templateId);
        //                        var blob = Storage.GetBlob(string.Format("inst-{0}", user.defaultInstanceID), $"templates/{template.Content}");
        //                        var fields = Module.Fields.OrderBy(x => x.Id).ToList();
        //                        var tempsName = templateName;
        //                        byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(tempsName);
        //                        var tempName = System.Text.Encoding.ASCII.GetString(bytes);
        //                        var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, moduleRepository);

        //                        var findRequest = new FindRequest();
        //                        findRequest.Fields = new List<string>();

        //                        for (int i = 0; i < fields.Count; i++)
        //                        {
        //                            var field = fields[i];

        //                            if (field.DataType != Model.Enums.DataType.Lookup)
        //                            {
        //                                findRequest.Fields.Add(field.Name);
        //                            }
        //                            else
        //                            {
        //                                var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
        //                                var primaryField = lookupModule.Fields.Single(x => x.Primary);
        //                                findRequest.Fields.Add(field.Name + "." + field.LookupType + "." + primaryField.Name);
        //                            }
        //                        }

        //                        var records = recordRepository.Find(moduleEntity.Name, findRequest);

        //                        using (var temp = new MemoryStream())
        //                        {
        //                            await blob.DownloadToStreamAsync(temp);
        //                            Workbook workbook = new Workbook(temp);
        //                            Worksheet worksheetReportAdd = workbook.Worksheets.Add("Report");
        //                            Worksheet worksheetData = workbook.Worksheets[0];
        //                            Worksheet worksheetReportFormul = workbook.Worksheets[1];
        //                            Worksheet worksheetReport = workbook.Worksheets["Report"];
        //                            var row = worksheetReportFormul.Cells.MaxDisplayRange.RowCount + 1;
        //                            var col = worksheetReportFormul.Cells.MaxDisplayRange.ColumnCount + 1;
        //                            var count = records.Count;

        //                            worksheetData.Cells.DeleteRows(0, count + 1);

        //                            DataTable dt = new DataTable("Excel");

        //                            for (int i = 0; i < fields.Count; i++)
        //                            {
        //                                var field = fields[i];
        //                                dt.Columns.Add(field.LabelTr.ToString());
        //                            }

        //                            for (int j = 0; j < records.Count; j++)
        //                            {
        //                                var record = records[j];
        //                                var dr = dt.NewRow();

        //                                for (int i = 0; i < fields.Count; i++)
        //                                {
        //                                    var field = fields[i];

        //                                    if (field.DataType != Model.Enums.DataType.Lookup)
        //                                    {
        //                                        dr[i] = record[field.Name];
        //                                    }
        //                                    else
        //                                    {
        //                                        var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
        //                                        var primaryField = lookupModule.Fields.Single(x => x.Primary);
        //                                        dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];
        //                                    }
        //                                }
        //                                dt.Rows.Add(dr);
        //                            }

        //                            worksheetData.Cells.ImportDataTable(dt, true, "A1");
        //                            workbook.CalculateFormula();
        //                            if (row > 0 && col > 0)
        //                            {
        //                                var fromRange = worksheetReportFormul.Cells.CreateRange(0, 0, row, col);
        //                                var toRange = worksheetReport.Cells.CreateRange(0, 0, 1, 1);
        //                                toRange.CopyValue(fromRange);
        //                            }
        //                            workbook.Worksheets.RemoveAt("Data");
        //                            workbook.Worksheets.RemoveAt("Report Formula");
        //                            workbook.Worksheets.RemoveAt("Evaluation Warning");

        //                            workbook.Save(System.Web.HttpContext.Current.Response, tempName + ".xlsx", ContentDisposition.Attachment, new OoxmlSaveOptions());
        //                            Response.End();

        //                            return null;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}

        //public async Task<FileStreamResult> ExportExcelData(string module, string templateName, int templateId, string locale = "", bool? normalize = false, int? timezoneOffset = 180)
        //{
        //    if (string.IsNullOrWhiteSpace(module) || templateId == null || templateId == 0)
        //        throw new HttpException(400, "Module field is required");

        //    using (var dbContext = new ApplicationDbContext())
        //    {
        //        var userId = AsyncHelpers.RunSync(() => Cache.ApplicationUser.Get(User.Identity.Name));
        //        var user = crmUser.GetById(userId);

        //        using (var tenantDbContext = new TenantDBContext(user.TenantID))
        //        {
        //            using (var moduleRepository = new ModuleRepository(tenantDbContext))
        //            {
        //                using (var templateRepostory = new TemplateRepository(tenantDbContext))
        //                {
        //                    using (var recordRepository = new RecordRepository(tenantDbContext))
        //                    {
        //                        var moduleEntity = await moduleRepository.GetByName(module);
        //                        var template = await templateRepostory.GetById(templateId);
        //                        var blob = Storage.GetBlob(string.Format("inst-{0}", user.defaultInstanceID), $"templates/{template.Content}");
        //                        var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
        //                        var tempsName = templateName;
        //                        byte[] bytes = Encoding.GetEncoding("Cyrillic").GetBytes(tempsName);
        //                        var tempName = Encoding.ASCII.GetString(bytes);
        //                        var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, moduleRepository);

        //                        var findRequest = new FindRequest();
        //                        findRequest.Fields = new List<string>();

        //                        for (int i = 0; i < fields.Count; i++)
        //                        {
        //                            var field = fields[i];

        //                            if (field.DataType != Model.Enums.DataType.Lookup)
        //                            {
        //                                findRequest.Fields.Add(field.Name);
        //                            }
        //                            else
        //                            {
        //                                var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
        //                                var primaryField = lookupModule.Fields.Single(x => x.Primary);
        //                                findRequest.Fields.Add(field.Name + "." + field.LookupType + "." + primaryField.Name);
        //                            }
        //                        }

        //                        var records = recordRepository.Find(moduleEntity.Name, findRequest);

        //                        using (var temp = new MemoryStream())
        //                        {
        //                            await blob.DownloadToStreamAsync(temp);
        //                            Workbook workbook = new Workbook(temp);
        //                            Worksheet worksheetReportAdd = workbook.Worksheets.Add("Report");
        //                            Worksheet worksheetData = workbook.Worksheets[0];
        //                            Worksheet worksheetReportFormul = workbook.Worksheets[1];
        //                            Worksheet worksheetReport = workbook.Worksheets["Report"];
        //                            var row = worksheetReportFormul.Cells.MaxDisplayRange.RowCount + 1;
        //                            var col = worksheetReportFormul.Cells.MaxDisplayRange.ColumnCount + 1;
        //                            var count = records.Count;

        //                            worksheetData.Cells.DeleteRows(0, count + 1);

        //                            DataTable dt = new DataTable("Excel");

        //                            for (int i = 0; i < fields.Count; i++)
        //                            {
        //                                var field = fields[i];
        //                                dt.Columns.Add(field.LabelTr.ToString());
        //                            }

        //                            for (int j = 0; j < records.Count; j++)
        //                            {
        //                                var record = records[j];
        //                                var dr = dt.NewRow();

        //                                for (int i = 0; i < fields.Count; i++)
        //                                {
        //                                    var field = fields[i];

        //                                    if (field.DataType != Model.Enums.DataType.Lookup)
        //                                    {
        //                                        dr[i] = record[field.Name];
        //                                    }
        //                                    else
        //                                    {
        //                                        var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
        //                                        var primaryField = lookupModule.Fields.Single(x => x.Primary);
        //                                        dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];
        //                                    }
        //                                }
        //                                dt.Rows.Add(dr);
        //                            }

        //                            worksheetData.Cells.ImportDataTable(dt, true, "A1");
        //                            workbook.CalculateFormula();
        //                            if (row > 0 && col > 0)
        //                            {
        //                                var fromRange = worksheetReportFormul.Cells.CreateRange(0, 0, row, col);
        //                                var toRange = worksheetReport.Cells.CreateRange(0, 0, 1, 1);
        //                                toRange.CopyValue(fromRange);
        //                            }

        //                            workbook.Worksheets.RemoveAt("Evaluation Warning");

        //                            workbook.Save(System.Web.HttpContext.Current.Response, tempName + ".xlsx", ContentDisposition.Attachment, new OoxmlSaveOptions());
        //                            Response.End();

        //                            return null;
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //}


    }




}

