using System;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using RecordHelper = PrimeApps.App.Helpers.RecordHelper;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers.QueryTranslation;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Web.Http;
using Microsoft.Extensions.Configuration;
using PrimeApps.App.Helpers;


namespace PrimeApps.App.Controllers
{
    [Route("api/convert"), Authorize]
    public class ConvertController : ApiBaseController
    {
        private IModuleRepository _moduleRepository;
        private IRecordRepository _recordRepository;
        private IProfileRepository _profileRepository;
        private IPicklistRepository _picklistRepository;
        private IDocumentRepository _documentRepository;
        private INoteRepository _noteRepository;
        private IConversionMappingRepository _conversionMappingRepository;
        private ITagRepository _tagRepository;
        private ISettingRepository _settingRepository;
        private Warehouse _warehouse;
        private IConfiguration _configuration;

        private IRecordHelper _recordHelper;

        public ConvertController(IModuleRepository moduleRepository, IRecordRepository recordRepository, IPicklistRepository picklistRepository, IProfileRepository profileRepository, IDocumentRepository documentRepository, INoteRepository noteRepository, IConversionMappingRepository conversionMappingRepository, ITagRepository tagRepository, ISettingRepository settingRepository, IRecordHelper recordHelper, IConfiguration configuration, Warehouse warehouse)
        {
            _moduleRepository = moduleRepository;
            _recordRepository = recordRepository;
            _profileRepository = profileRepository;
            _picklistRepository = picklistRepository;
            _documentRepository = documentRepository;
            _noteRepository = noteRepository;
            _conversionMappingRepository = conversionMappingRepository;
            _tagRepository = tagRepository;
            _settingRepository = settingRepository;
            _warehouse = warehouse;

            _recordHelper = recordHelper;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_profileRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_picklistRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_documentRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_noteRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_conversionMappingRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("create_mapping"), HttpPost]
        public async Task<IActionResult> CreateLeadConversionMapping([FromBody]ConversionMapping conversionMapping)
        {
            if (ModelState.IsValid)
            {
                var mappedObject = await _conversionMappingRepository.GetByFields(conversionMapping);
                if (mappedObject != null)
                {
                    await _conversionMappingRepository.DeleteHard(mappedObject);
                }

                await _conversionMappingRepository.Create(conversionMapping);
                return Ok();
            }

            return BadRequest();
        }

        [Route("delete_mapping"), HttpPost]
        public async Task<IActionResult> Delete([FromBody]ConversionMapping conversionMapping)
        {
            var mappedObject = await _conversionMappingRepository.GetByFields(conversionMapping);
            if (mappedObject != null)
            {
                await _conversionMappingRepository.DeleteHard(mappedObject);
            }

            return Ok();
        }

        [Route("get_mappings/{moduleId:int}"), HttpGet]
        public async Task<IActionResult> GetModuleMappings(int moduleId)
        {
            var results = await _conversionMappingRepository.GetAll(moduleId);
            return Ok(results);
        }

        [Route("delete_fields_mappings"), HttpPost]
        public async Task<IActionResult> DeleteFieldsMappings([FromBody]List<int> fieldsId)
        {
            foreach (var field in fieldsId)
            {
                var conversionMappings = await _conversionMappingRepository.GetMappingsByFieldId(field);

                foreach (ConversionMapping _conversionMapping in conversionMappings)
                    await _conversionMappingRepository.DeleteSoft(_conversionMapping);
            }

            return Ok();
        }

        [Route("convert_candidate")]
        [HttpPost]
        public async Task<IActionResult> ConvertPersonal([FromBody]JObject request)
        {
            var candidateModule = await _moduleRepository.GetByName("adaylar");
            var employeeModule = await _moduleRepository.GetByName("calisanlar");
            var activityModule = await _moduleRepository.GetByName("activities");
            var candidate = await _recordRepository.GetById(candidateModule, (int)request["lead_id"], false, profileBasedEnabled: false);
            var conversionMappings = await _conversionMappingRepository.GetAll(candidateModule.Id);
            var employee = new JObject();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            if (conversionMappings == null || conversionMappings.Count < 1)
                return BadRequest("Conversion mappings not found.");

            // Prepare new records
            foreach (var conversionMapping in conversionMappings)
            {
                var field = candidateModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.FieldId);
                var mappingField = conversionMapping.MappingModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.MappingFieldId);

                switch (conversionMapping.MappingModule.Name)
                {
                    case "calisanlar":
                        if (field != null && mappingField != null && !candidate[field.Name].IsNullOrEmpty())
                            employee[mappingField.Name] = candidate[field.Name];
                        break;
                }
            }

            // Create new records
            // account
            if (employee.HasValues)
            {
                employee["owner"] = candidate["owner"];
                employee["master_id"] = candidate["id"];
                var resultBefore = await _recordHelper.BeforeCreateUpdate(employeeModule, employee, ModelState, AppUser.TenantLanguage, _moduleRepository, _picklistRepository, _profileRepository, _tagRepository, _settingRepository, _recordRepository, false, appUser: AppUser);

                if (resultBefore != HttpStatusCode.Status200OK && !ModelState.IsValid)
                    return StatusCode(resultBefore, ModelState);

                int resultCreate;

                try
                {
                    resultCreate = await _recordRepository.Create(employee, employeeModule);
                }
                catch (PostgresException ex)
                {
                    if (ex.SqlState == PostgreSqlStateCodes.UniqueViolation)
                        return StatusCode(HttpStatusCode.Status409Conflict, _recordHelper.PrepareConflictError(ex));

                    if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                        return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.Detail });

                    if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                        return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                    throw;
                }

                if (resultCreate < 1)
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

                _recordHelper.AfterCreate(employeeModule, employee, AppUser, _warehouse);
            }

            // Update lead as converted
            candidate["is_converted"] = true;

            var convertedField = candidateModule.Fields.FirstOrDefault(x => x.Name == "converted" && !x.Deleted);

            if (convertedField != null)
                candidate["converted"] = true;

            if ((bool)request["deleted"] == true)
                candidate["deleted"] = true;

            var leadModel = new JObject();
            int resultUpdate;

            foreach (var pair in candidate)
            {
                if (!pair.Value.IsNullOrEmpty())
                    leadModel[pair.Key] = candidate[pair.Key];
            }

            try
            {
                resultUpdate = await _recordRepository.Update(leadModel, candidateModule, (bool)request["deleted"]);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.UniqueViolation)
                    return StatusCode(HttpStatusCode.Status409Conflict, _recordHelper.PrepareConflictError(ex));

                if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.Detail });

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                throw;
            }

            if (resultUpdate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            _recordHelper.AfterDelete(candidateModule, leadModel, AppUser, _warehouse);

            // Move documents
            var documents = await _documentRepository.GetAll(candidateModule.Id, (int)candidate["id"]);

            foreach (var document in documents)
            {
                document.ModuleId = employeeModule.Id;
                document.RecordId = (int)employee["id"];

                await _documentRepository.Update(document);
            }

            // Move activities
            var findRequestActivity = new FindRequest
            {
                Fields = new List<string> { "related_to" },
                Filters = new List<Filter> { new Filter { Field = "related_to", Operator = Operator.Equals, Value = (int)candidate["id"], No = 1 } },
                Limit = 1000,
                Offset = 0
            };

            var activities = await _recordRepository.Find("activities", findRequestActivity, false, false);

            foreach (JObject activity in activities)
            {
                activity["related_to"] = (int)employee["id"];
                await _recordRepository.Update(activity, activityModule);
            }

            // Move notes
            var noteRequest = new NoteRequest
            {
                ModuleId = candidateModule.Id,
                RecordId = (int)candidate["id"],
                Limit = 1000,
                Offset = 0
            };

            var notes = await _noteRepository.Find(noteRequest);

            foreach (var note in notes)
            {
                note.ModuleId = employeeModule.Id;
                note.RecordId = (int)employee["id"];
                await _noteRepository.Update(note);
            }

            //Move dynamic sub modules
            var conversionsubModules = await _conversionMappingRepository.GetSubConversions(candidateModule.Id);
            foreach (var conversion in conversionsubModules)
            {
                var subModule = await _moduleRepository.GetByName(conversion.MappingSubModule.Name);
                var findRequest = new FindRequest
                {
                    Filters = new List<Filter> { new Filter { Field = conversion.SubModuleSourceField, Operator = Operator.Equals, Value = (int)candidate["id"], No = 1 } },
                    Limit = 1000,
                    Offset = 0
                };

                var subModuleRecords = await _recordRepository.Find(conversion.MappingSubModule.Name, findRequest, false, false);

                foreach (JObject subRecord in subModuleRecords)
                {
                    subRecord[conversion.SubModuleDestinationField] = (int)employee["id"];
                    await _recordRepository.Update(subRecord, conversion.MappingSubModule);
                }
            }


            var result = new JObject();
            result["account_id"] = employee["id"];

            return Ok(result);
        }
    }
}