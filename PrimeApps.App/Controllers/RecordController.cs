using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Enums;
using RecordHelper = PrimeApps.App.Helpers.RecordHelper;
using ModuleHelper = PrimeApps.Model.Helpers.ModuleHelper;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Constants;
using PrimeApps.Model.Helpers.QueryTranslation;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.App.Controllers
{
    [Route("api/record"), Authorize]
    public class RecordController : ApiBaseController
    {
        private IRecordRepository _recordRepository;
        private IModuleRepository _moduleRepository;
        private IPicklistRepository _picklistRepository;
        private IProcessRepository _processRepository;
        private Warehouse _warehouse;
        private IConfiguration _configuration;

        private IRecordHelper _recordHelper;

        public RecordController(IRecordRepository recordRepository, IModuleRepository moduleRepository, IPicklistRepository picklistRepository, IRecordHelper recordHelper, Warehouse warehouse, IConfiguration configuration, IProcessRepository processRepository)
        {
            _recordRepository = recordRepository;
            _moduleRepository = moduleRepository;
            _picklistRepository = picklistRepository;
            _processRepository = processRepository;
            _warehouse = warehouse;

            _recordHelper = recordHelper;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_recordRepository);
            SetCurrentUser(_moduleRepository);
            SetCurrentUser(_picklistRepository);

            base.OnActionExecuting(context);
        }

        [Route("get/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}/{id:int}"), HttpGet]
        public async Task<IActionResult> Get(string module, int id, [FromQuery(Name = "locale")]string locale = "", [FromQuery(Name = "normalize")] bool? normalize = false, [FromQuery(Name = "timezoneOffset")]int? timezoneOffset = 180)
        {
            JObject record;
            var moduleEntity = await _moduleRepository.GetByNameBasic(module);

            if (module == "users")
                moduleEntity = ModuleHelper.GetFakeUserModule();

            if (moduleEntity == null)
                return BadRequest();

            var lookupModuleNames = new List<string>();
            var deletedModulLookupFields = new List<Field>();

            foreach (var field in moduleEntity.Fields)
            {
                if (field.DataType == DataType.Lookup && field.LookupType != "users" && field.LookupType != "relation" && !lookupModuleNames.Contains(field.LookupType))
                {
                    var lookupModule = await _moduleRepository.GetByName(field.LookupType);

                    if (lookupModule != null)
                        lookupModuleNames.Add(field.LookupType);
                    else
                        deletedModulLookupFields.Add(field);
                }
            }

            //Remove field if it use deleted modul for lookup.
            foreach (var deletedfield in deletedModulLookupFields)
            {
                moduleEntity.Fields.Remove(deletedfield);
            }

            ICollection<Module> lookupModules = new List<Module>();

            if (lookupModuleNames.Count > 0)
            {
                lookupModules = await _moduleRepository.GetByNamesBasic(lookupModuleNames);

                if (lookupModules.Count < lookupModuleNames.Count)
                    return NotFound();
            }

            lookupModules.Add(ModuleHelper.GetFakeUserModule());

            try
            {
                record = _recordRepository.GetById(moduleEntity, id, !AppUser.HasAdminProfile, lookupModules);

                if (record == null)
                    return NotFound();

                //Format records if has locale
                if (!string.IsNullOrWhiteSpace(locale))
                {
                    var currentCulture = locale == "en" ? "en-US" : "tr-TR";
                    record = await Model.Helpers.RecordHelper.FormatRecordValues(moduleEntity, record, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset.Value, lookupModules);

                    if (normalize.HasValue && normalize.Value)
                        record = Model.Helpers.RecordHelper.NormalizeRecordValues(record);
                }

                //if field encrypted, controls permission and decrypts
                foreach (var field in moduleEntity.Fields)
                {
                    if (field.Encrypted && !record[field.Name + "__encrypted"].IsNullOrEmpty())
                    {
                        bool hasEncryptPermission = false;
                        if (field.EncryptionAuthorizedUsersList.Count > 0)
                        {
                            foreach (var user in field.EncryptionAuthorizedUsersList)
                            {
                                if (user == AppUser.Id.ToString())
                                    hasEncryptPermission = true;
                            }
                        }

                        if (hasEncryptPermission)
                            record[field.Name] = EncryptionHelper.Decrypt((string)record[field.Name + "__encrypted"], field.Id, AppUser, _configuration);
                        else record[field.Name] = "***************";
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.UndefinedTable)
                    return NotFound();

                throw;
            }

            return Ok(record);
        }

        [Route("find/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpPost]
        public async Task<IActionResult> Find(string module, [FromBody]FindRequest request, [FromQuery(Name = "locale")]string locale = "", [FromQuery(Name = "normalize")]bool? normalize = false, [FromQuery(Name = "timezoneOffset")]int? timezoneOffset = 180)
        {
            if (request == null)
                request = new FindRequest();

            if (!_recordHelper.ValidateFilterLogic(request.FilterLogic, request.Filters))
                ModelState.AddModelError("request._filter_logic", "The field FilterLogic is invalid or has no filters.");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            JArray records;

            //Full Text Search
            try
            {
                ICollection<Filter> customQueryFilters = new List<Filter>();

                if (request.Filters?.FirstOrDefault(x => x.DocumentSearch) != null)
                {
                    //Custom filter logic starts here. Null value equals DataType.Custom from request.Filters

                    request.Filters.Where(v => v.DocumentSearch).ToList().ForEach(x =>
                    {
                        var field = x.Field;

                        if (field != null)
                        {
                            //remove custom filter to not search on database, target search is custom, currently Azure
                            request.Filters.Remove(x);

                            customQueryFilters.Add(x);
                        }
                    });
                }

                records = _recordRepository.Find(module, request, timezoneOffset: timezoneOffset.Value);

                //Format records if has locale
                if (!string.IsNullOrWhiteSpace(locale))
                {
                    var currentCulture = locale == "en" ? "en-US" : "tr-TR";
                    var moduleEntity = await _moduleRepository.GetByName(module);
                    var isManyToMany = !string.IsNullOrWhiteSpace(request.ManyToMany);
                    var additionalModule = isManyToMany ? request.ManyToMany : string.Empty;
                    var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository, additionalModule);
                    var recordsFiltered = new JArray();

                    if (isManyToMany)
                        lookupModules.Add(moduleEntity);

                    foreach (JObject record in records)
                    {
                        var newRecord = await Model.Helpers.RecordHelper.FormatRecordValues(moduleEntity, record, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset.Value, lookupModules);

                        if (normalize.HasValue && normalize.Value)
                            newRecord = Model.Helpers.RecordHelper.NormalizeRecordValues(newRecord, isManyToMany);

                        recordsFiltered.Add(newRecord);
                    }

                    records = recordsFiltered;
                }

                //Full Text Search - Custom filter merge operation with manipulating records data on for non total count object(s)
                var filteredRecords = new JArray();
                if (request.Fields != null && request.Fields.FirstOrDefault(x => x == "total_count()") == null)
                {
                    if (customQueryFilters.Count > 0)
                    {
                        var ids = new List<string>();

                        foreach (var data in records)
                        {
                            var id = data["id"]?.ToString();
                            ids.Add(id);
                        }

                        if (ids.Count > 0)
                        {
                            var searchIndexName = AppUser.TenantGuid + "-" + module;
                            var documentSearch = new DocumentSearch();
                            var documentRecords = documentSearch.SearchDocuments(ids, searchIndexName, customQueryFilters, _configuration);

                            foreach (var record in records)
                            {
                                foreach (var documentRecord in documentRecords)
                                {
                                    if (record["id"]?.ToString() == documentRecord.ToString())
                                    {
                                        filteredRecords.Add(record);
                                    }
                                }
                            }

                            records = filteredRecords;
                        }
                    }
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.UndefinedTable)
                    return NotFound();

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                if (ex.SqlState == PostgreSqlStateCodes.InvalidInput)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                throw;
            }

            return Ok(records);
        }

        [Route("create/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpPost]
        public async Task<IActionResult> Create(string module, [FromBody]JObject record, [FromQuery(Name = "locale")]string locale = "", [FromQuery(Name = "normalize")]bool? normalize = false, [FromQuery(Name = "timezoneOffset")]int timezoneOffset = 180)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetByName(module);

            if (moduleEntity == null || record == null)
                return BadRequest();

            var resultBefore = await _recordHelper.BeforeCreateUpdate(moduleEntity, record, ModelState, AppUser.TenantLanguage, appUser: AppUser);

            if (resultBefore < 0 && !ModelState.IsValid)
                return BadRequest(ModelState);

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            int resultCreate;

            try
            {
                resultCreate = await _recordRepository.Create(record, moduleEntity);

                // If module is opportunities create stage history
                if (module == "opportunities")
                    await _recordHelper.CreateStageHistory(record);
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

            //Check number auto fields and combinations and update record with combined values
            var numberAutoFields = moduleEntity.Fields.Where(x => x.DataType == DataType.NumberAuto).ToList();

            if (numberAutoFields.Count > 0)
            {
                var currentRecord = _recordRepository.GetById(moduleEntity, (int)record["id"], AppUser.HasAdminProfile);
                var hasUpdate = false;

                foreach (var numberAutoField in numberAutoFields)
                {
                    var combinationFields = moduleEntity.Fields.Where(x => x.Combination != null && (x.Combination.Field1 == numberAutoField.Name || x.Combination.Field2 == numberAutoField.Name)).ToList();

                    if (combinationFields.Count > 0)
                    {
                        foreach (var combinationField in combinationFields)
                        {
                            _recordHelper.SetCombinations(currentRecord, null, combinationField);
                        }

                        hasUpdate = true;
                    }
                }

                if (hasUpdate)
                {
                    var recordUpdate = new JObject();
                    recordUpdate["id"] = (int)record["id"];

                    var combinationFields = moduleEntity.Fields.Where(x => x.Combination != null).ToList();

                    foreach (var combinationField in combinationFields)
                    {
                        recordUpdate[combinationField.Name] = currentRecord[combinationField.Name];
                    }

                    await _recordRepository.Update(recordUpdate, moduleEntity);
                }
            }

            //After create
            _recordHelper.AfterCreate(moduleEntity, record, AppUser, _warehouse, timeZoneOffset: timezoneOffset);

            //Format records if has locale
            if (!string.IsNullOrWhiteSpace(locale))
            {
                ICollection<Module> lookupModules = new List<Module> { ModuleHelper.GetFakeUserModule() };
                var currentCulture = locale == "en" ? "en-US" : "tr-TR";
                record = _recordRepository.GetById(moduleEntity, (int)record["id"], !AppUser.HasAdminProfile, lookupModules);
                record = await Model.Helpers.RecordHelper.FormatRecordValues(moduleEntity, record, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset, lookupModules);

                if (normalize.HasValue && normalize.Value)
                    record = Model.Helpers.RecordHelper.NormalizeRecordValues(record);
            }

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/record/get/" + module + "/?id=" + record["id"], record);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/record/get/" + module + "/?id=" + record["id"], record);
        }

        [Route("update/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpPut]
        public async Task<IActionResult> Update(string module, [FromBody]JObject record, [FromQuery(Name = "runWorkflows")]bool runWorkflows = true, [FromQuery(Name = "locale")]string locale = "", [FromQuery(Name = "normalize")]bool? normalize = false, int timezoneOffset = 180)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetByName(module);

            if (moduleEntity == null || record == null)
                return BadRequest();

            if (record["id"] == null || (int)record["id"] < 1)
                return BadRequest("Record id is required!");

            int recordId;
            if (!int.TryParse((string)record["id"], out recordId) || recordId < 1)
                return BadRequest("Invalid record id!");

            var currentRecord = _recordRepository.GetById(moduleEntity, (int)record["id"], AppUser.HasAdminProfile);

            if (currentRecord == null)
                return BadRequest("Record not found!");

            var resultBefore = await _recordHelper.BeforeCreateUpdate(moduleEntity, record, ModelState, AppUser.TenantLanguage, true, currentRecord, AppUser);

            //if ((bool)record["freeze"])
            //    return StatusCode(HttpStatusCode.Status403Forbidden);

            if (resultBefore < 0 && !ModelState.IsValid)
                return BadRequest(ModelState);

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            int resultUpdate;

            try
            {
                resultUpdate = await _recordRepository.Update(record, moduleEntity);

                // If module is opportunities update stage history
                if (module == "opportunities")
                    await _recordHelper.UpdateStageHistory(record, currentRecord);
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

            _recordHelper.AfterUpdate(moduleEntity, record, currentRecord, AppUser, _warehouse, runWorkflows, timeZoneOffset: timezoneOffset);

            //Format records if has locale
            if (!string.IsNullOrWhiteSpace(locale))
            {
                ICollection<Module> lookupModules = new List<Module> { ModuleHelper.GetFakeUserModule() };
                var currentCulture = locale == "en" ? "en-US" : "tr-TR";
                record = _recordRepository.GetById(moduleEntity, (int)record["id"], !AppUser.HasAdminProfile, lookupModules);
                record = await Model.Helpers.RecordHelper.FormatRecordValues(moduleEntity, record, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset, lookupModules);

                if (normalize.HasValue && normalize.Value)
                    record = Model.Helpers.RecordHelper.NormalizeRecordValues(record);
            }

            return Ok(record);
        }

        [Route("delete/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}/{id:int}"), HttpDelete]
        public async Task<IActionResult> Delete(string module, int id)
        {
            var moduleEntity = await _moduleRepository.GetByName(module);
            var record = _recordRepository.GetById(moduleEntity, id, !AppUser.HasAdminProfile);

            if (moduleEntity == null || record == null)
                return BadRequest();

            var resultBefore = await _recordHelper.BeforeDelete(moduleEntity, record, AppUser, _processRepository, _warehouse);

            if (!record["freeze"].IsNullOrEmpty() && (bool)record["freeze"])
                return StatusCode(HttpStatusCode.Status403Forbidden);

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            await _recordRepository.Delete(record, moduleEntity);

            _recordHelper.AfterDelete(moduleEntity, record, AppUser, _warehouse);

            return Ok();
        }

        [Route("create_bulk/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpPost]
        public async Task<IActionResult> CreateBulk(string module, [FromBody]JArray records)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetByName(module);

            foreach (JObject record in records)
            {
                if (moduleEntity == null || record == null)
                    return BadRequest();

                var resultBefore = await _recordHelper.BeforeCreateUpdate(moduleEntity, record, ModelState, AppUser.TenantLanguage);

                if (resultBefore < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

                //Set warehouse database name
                _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

                int resultCreate;

                try
                {
                    resultCreate = await _recordRepository.Create(record, moduleEntity);

                    // If module is opportunities create stage history
                    if (module == "opportunities")
                        await _recordHelper.CreateStageHistory(record);
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

                //After create
                _recordHelper.AfterCreate(moduleEntity, record, AppUser, _warehouse, runDefaults: false, runWorkflows: false, runCalculations: true);

            }

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/record/find/" + module, records);
            //return Created(Request.Scheme + "://" + Request.Host + "/api/record/find/" + module, records);
        }

        [Route("delete_bulk/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpDelete]
        public async Task<IActionResult> DeleteBulk(string module, [FromBody]int[] ids)
        {
            var moduleEntity = await _moduleRepository.GetByName(module);

            foreach (var id in ids)
            {
                var record = _recordRepository.GetById(moduleEntity, id, !AppUser.HasAdminProfile);

                if (moduleEntity == null || record == null)
                    return BadRequest();

                await _recordHelper.BeforeDelete(moduleEntity, record, AppUser, _processRepository, _warehouse);


                if (!record["freeze"].IsNullOrEmpty() && (bool)record["freeze"])
                    return StatusCode(HttpStatusCode.Status403Forbidden);

                //Set warehouse database name
                _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

                await _recordRepository.Delete(record, moduleEntity);

                _recordHelper.AfterDelete(moduleEntity, record, AppUser, _warehouse);

            }

            return Ok();
        }


        [Route("update_bulk/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpPut]
        public async Task<IActionResult> UpdateBulk(string module, [FromBody]JObject request)
        {
            var moduleEntity = await _moduleRepository.GetByName(module);
            var ids = (JArray)request["ids"];
            var record = (JObject)request["record"];

            foreach (var id in ids)
            {
                int recordId;
                if (!int.TryParse(id.ToString(), out recordId) || recordId < 1)
                    return BadRequest("Invalid record id!");

                var currentRecord = _recordRepository.GetById(moduleEntity, (int)id, AppUser.HasAdminProfile);
                var recordUpdate = (JObject)record.DeepClone();

                if (currentRecord.IsNullOrEmpty())
                    return BadRequest("Record not found!");

                var resultBefore = await _recordHelper.BeforeCreateUpdate(moduleEntity, recordUpdate, ModelState, AppUser.TenantLanguage, true, currentRecord, AppUser);

                if (resultBefore < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

                //Set warehouse database name
                _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

                int resultUpdate;

                try
                {
                    recordUpdate["id"] = recordId;
                    resultUpdate = await _recordRepository.Update(recordUpdate, moduleEntity);

                    // If module is opportunities update stage history
                    if (module == "opportunities")
                        await _recordHelper.UpdateStageHistory(recordUpdate, currentRecord);
                }
                catch (PostgresException ex)
                {
                    if (ex.SqlState == PostgreSqlStateCodes.UniqueViolation)
                        return StatusCode(HttpStatusCode.Status409Conflict, _recordHelper.PrepareConflictError(ex));

                    if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                        return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                    if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)

                        return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                    throw;
                }

                if (resultUpdate < 1)
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

                _recordHelper.AfterUpdate(moduleEntity, recordUpdate, currentRecord, AppUser, _warehouse);
            }

            return Ok();
        }

        [Route("add_relations/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}/{relatedModule:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpPost]
        public async Task<IActionResult> AddRelations(string module, string relatedModule, [FromBody]JArray records)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var relationId = 0;

            if (relatedModule.Contains("|"))
            {
                var parts = relatedModule.Split('|');
                relatedModule = parts[0];

                if (int.TryParse(parts[1], out relationId))
                    return BadRequest("Relation Id must be integer");
            }

            var moduleEntity = await _moduleRepository.GetByName(module);
            var relatedModuleEntity = await _moduleRepository.GetByName(relatedModule);

            if (moduleEntity == null || relatedModuleEntity == null || records == null || records.Count < 1)
                return BadRequest();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            int resultAddRelations;

            try
            {
                resultAddRelations = await _recordRepository.AddRelations(records, moduleEntity.Name, relatedModuleEntity.Name, relationId);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.Detail });

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                throw;
            }

            if (resultAddRelations < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            return Ok(resultAddRelations);
        }

        [Route("delete_relation/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}/{relatedModule:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpDelete]
        public async Task<IActionResult> DeleteRelation(string module, string relatedModule, [FromBody]JObject record)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetByName(module);
            var relatedModuleEntity = await _moduleRepository.GetByName(relatedModule);

            if (moduleEntity == null || relatedModuleEntity == null || record == null)
                return BadRequest();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            int resultDeleteRelation;

            try
            {
                resultDeleteRelation = await _recordRepository.DeleteRelation(record, moduleEntity.Name, relatedModuleEntity.Name);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                throw;
            }

            if (resultDeleteRelation < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            return Ok(resultDeleteRelation);
        }

        [Route("get_lookup_ids"), HttpPost]
        public IActionResult GetLookupIds([FromBody]JArray lookupRequest)
        {
            var lookups = _recordRepository.GetLookupIds(lookupRequest);

            return Ok(lookups);
        }

        [Route("lookup_user"), HttpPost]
        public IActionResult LookupUser([FromBody]LookupUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var records = _recordRepository.LookupUser(request);

            return Ok(records);
        }

        [Route("get_all_by_id/{module:regex(" + AlphanumericConstants.AlphanumericUnderscoreRegex + ")}"), HttpPost]
        public async Task<IActionResult> GetAllById(string module, [FromBody]int[] recordIds, [FromQuery(Name = "locale")]string locale = "", [FromQuery(Name = "normalize")]bool? normalize = false, [FromQuery(Name = "timezoneOffset")]int? timezoneOffset = 180)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            JArray records;

            try
            {
                records = _recordRepository.GetAllById(module, recordIds.ToList());

                //Format records if has locale
                if (!string.IsNullOrWhiteSpace(locale))
                {
                    var currentCulture = locale == "en" ? "en-US" : "tr-TR";
                    var moduleEntity = await _moduleRepository.GetByName(module);
                    var recordsFiltered = new JArray();

                    foreach (JObject record in records)
                    {
                        var newRecord = await Model.Helpers.RecordHelper.FormatRecordValues(moduleEntity, record, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset.Value, null);

                        if (normalize.HasValue && normalize.Value)
                            newRecord = Model.Helpers.RecordHelper.NormalizeRecordValues(newRecord, true);

                        recordsFiltered.Add(newRecord);
                    }

                    records = recordsFiltered;
                }
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.UndefinedTable)
                    return NotFound();

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                if (ex.SqlState == PostgreSqlStateCodes.InvalidInput)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                throw;
            }

            return Ok(records);
        }
    }
}
