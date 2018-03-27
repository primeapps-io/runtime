using System.Threading.Tasks;
using System.Web.Http;
using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.AuditLog;
using PrimeApps.Model.Common.Document;
using PrimeApps.Model.Common.Import;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using RecordHelper = PrimeApps.App.Helpers.RecordHelper;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Helpers.QueryTranslation;

namespace PrimeApps.App.Controllers
{
    [RoutePrefix("api/data"), Authorize, SnakeCase]
    public class DataController : BaseController
    {
        private IAuditLogRepository _auditLogRepository;
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private IImportRepository _importRepository;
        private ITenantRepository _tenantRepository;
        private Warehouse _warehouse;

        public DataController(IAuditLogRepository auditLogRepository, IRecordRepository recordRepository, IModuleRepository moduleRepository, IImportRepository importRepository, ITenantRepository tenantRepository, Warehouse warehouse)
        {
            _auditLogRepository = auditLogRepository;
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _importRepository = importRepository;
            _tenantRepository = tenantRepository;
            _warehouse = warehouse;
        }

        [Route("import/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpPost]
        public async Task<IHttpActionResult> Import([FromUri]string module, [FromBody]JArray records)
        {
            var moduleEntity = await _moduleRepository.GetByName(module);

            if (moduleEntity == null || records.IsNullOrEmpty() || records.Count < 1)
                return BadRequest();

            var importEntity = new Import
            {
                ModuleId = moduleEntity.Id,
                TotalCount = records.Count
            };

            var result = await _importRepository.Create(importEntity);

            if (result < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            foreach (JObject record in records)
            {
                record["import_id"] = importEntity.Id;
            }

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            int resultCreate;

            try
            {
                resultCreate = await _recordRepository.CreateBulk(records, moduleEntity);
            }
            catch (PostgresException ex)
            {
                await _importRepository.DeleteHard(importEntity);

                if (ex.SqlState == PostgreSqlStateCodes.UniqueViolation)
                    return Content(HttpStatusCode.Conflict, RecordHelper.PrepareConflictError(ex));

                if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.Detail });

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.MessageText });

                throw;
            }

            if (resultCreate < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            return Ok(importEntity);
        }

        [Route("import_save_excel"), HttpPost]
        public async Task<IHttpActionResult> ImportSaveExcel([FromUri]int importId)
        {
            var import = await _importRepository.GetById(importId);

            if (import == null)
                return NotFound();

            var stream = await Request.Content.ReadAsStreamAsync();
            DocumentUploadResult result;
            var isUploaded = DocumentHelper.Upload(stream, out result);

            if (!isUploaded)
                return BadRequest();

            var excelUrl = DocumentHelper.Save(result, "import-" + AppUser.TenantId);

            import.ExcelUrl = excelUrl;
            await _importRepository.Update(import);

            return Ok(excelUrl);
        }

        [Route("import_find"), HttpPost]
        public async Task<ICollection<Import>> ImportFind(ImportRequest request)
        {
            return await _importRepository.Find(request);
        }

        [Route("import_revert/{importId:int}"), HttpDelete]
        public async Task<IHttpActionResult> RevertImport([FromUri]int importId)
        {
            var import = await _importRepository.GetById(importId);

            if (import == null)
                return NotFound();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            await _importRepository.Revert(import);
            await _importRepository.DeleteSoft(import);

            return Ok();
        }

        [Route("remove_sample_data"), HttpDelete]
        public async Task<IHttpActionResult> RemoveSampleData()
        {
            var result = await _recordRepository.DeleteSampleData((List<Module>) await _moduleRepository.GetAll());

            if (result > 0)
            {
                var instanceToUpdate = await _tenantRepository.GetAsync(AppUser.TenantId);
                instanceToUpdate.HasSampleData = false;
               await _tenantRepository.UpdateAsync(instanceToUpdate);
            }

            return Ok();
        }

        [Route("find_audit_logs"), HttpPost]
        public async Task<ICollection<AuditLog>> FindAuditLogs(AuditLogRequest request)
        {
            return await _auditLogRepository.Find(request);
        }
    }
}
