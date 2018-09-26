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
using System;
using Microsoft.Extensions.Configuration;
using PrimeApps.App.Helpers;


namespace PrimeApps.App.Controllers
{
    [Route("api/convert"), Authorize]
    public class ConvertController : ApiBaseController
    {
        private IModuleRepository _moduleRepository;
        private IRecordRepository _recordRepository;
        private IPicklistRepository _picklistRepository;
        private IDocumentRepository _documentRepository;
        private INoteRepository _noteRepository;
        private IConversionMappingRepository _conversionMappingRepository;
        private Warehouse _warehouse;
        private IConfiguration _configuration;

	    private IRecordHelper _recordHelper;

        public ConvertController(IModuleRepository moduleRepository, IRecordRepository recordRepository, IPicklistRepository picklistRepository, IDocumentRepository documentRepository, INoteRepository noteRepository, IConversionMappingRepository conversionMappingRepository, IRecordHelper recordHelper, IConfiguration configuration, Warehouse warehouse)
        {
            _moduleRepository = moduleRepository;
            _recordRepository = recordRepository;
            _picklistRepository = picklistRepository;
            _documentRepository = documentRepository;
            _noteRepository = noteRepository;
            _conversionMappingRepository = conversionMappingRepository;
            _warehouse = warehouse;

	        _recordHelper = recordHelper;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_moduleRepository);
            SetCurrentUser(_recordRepository);
            SetCurrentUser(_picklistRepository);
            SetCurrentUser(_documentRepository);
            SetCurrentUser(_noteRepository);
            SetCurrentUser(_conversionMappingRepository);

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
        [Route("convert_lead")]
        [HttpPost]
        public async Task<IActionResult> ConvertLead([FromBody]JObject request)
        {
            var leadModule = await _moduleRepository.GetByName("leads");
            var accountModule = await _moduleRepository.GetByName("accounts");
            var contactModule = await _moduleRepository.GetByName("contacts");
            var opportunityModule = await _moduleRepository.GetByName("opportunities");
            var activityModule = await _moduleRepository.GetByName("activities");
            var lead = _recordRepository.GetById(leadModule, (int)request["lead_id"], false);
            var conversionMappings = await _conversionMappingRepository.GetAll(leadModule.Id);
            var account = new JObject();
            var contact = new JObject();
            var opportunity = new JObject();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            if (conversionMappings == null || conversionMappings.Count < 1)
                return BadRequest("Conversion mappings not found.");

            // Prepare new records
            foreach (var conversionMapping in conversionMappings)
            {
                var field = leadModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.FieldId);
                var mappingField = conversionMapping.MappingModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.MappingFieldId);

                switch (conversionMapping.MappingModule.Name)
                {
                    case "accounts":
                        if (field != null && mappingField != null && !lead[field.Name].IsNullOrEmpty())
                            account[mappingField.Name] = lead[field.Name];
                        break;
                    case "contacts":
                        if (field != null && mappingField != null && !lead[field.Name].IsNullOrEmpty())
                            contact[mappingField.Name] = lead[field.Name];
                        break;
                    case "opportunities":
                        if (field != null && mappingField != null && !lead[field.Name].IsNullOrEmpty())
                            opportunity[mappingField.Name] = lead[field.Name];
                        break;
                }
            }

            if (request["opportunity"] != null)
            {
                opportunity["name"] = request["opportunity"]["name"];
                opportunity["amount"] = request["opportunity"]["amount"];
                opportunity["closing_date"] = request["opportunity"]["closing_date"];
                opportunity["stage"] = request["opportunity"]["stage"];
            }
            else
            {
                opportunity = null;
            }

            // Create new records
            // account
            if (account.HasValues)
            {
                account["owner"] = lead["owner"];
                account["master_id"] = lead["id"];
                var resultBefore = await _recordHelper.BeforeCreateUpdate(accountModule, account, ModelState, AppUser.TenantLanguage, false);

                if (resultBefore < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

                int resultCreate;

                try
                {
                    resultCreate = await _recordRepository.Create(account, accountModule);
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

	            _recordHelper.AfterCreate(accountModule, account, AppUser, _warehouse);
            }

            // contact
            if (contact.HasValues)
            {
                contact["owner"] = lead["owner"];
                contact["master_id"] = lead["id"];

                if (account["id"] != null && contactModule.Fields.Any(x => x.Name == "account"))
                    contact["account"] = account["id"];

                var resultBefore = await _recordHelper.BeforeCreateUpdate(contactModule, contact, ModelState, AppUser.TenantLanguage, false);

                if (resultBefore < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

                int resultCreate;

                try
                {
                    resultCreate = await _recordRepository.Create(contact, contactModule);
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

	            _recordHelper.AfterCreate(contactModule, contact, AppUser, _warehouse);
            }

            // opportunity
            if (opportunity != null && opportunity.HasValues)
            {
                opportunity["owner"] = lead["owner"];
                opportunity["master_id"] = lead["id"];

                if (account["id"] != null && opportunityModule.Fields.Any(x => x.Name == "account"))
                    opportunity["account"] = account["id"];

                if (contact["id"] != null && opportunityModule.Fields.Any(x => x.Name == "contact"))
                    opportunity["contact"] = contact["id"];

                var resultBefore = await _recordHelper.BeforeCreateUpdate(opportunityModule, opportunity, ModelState, AppUser.TenantLanguage);

                if (resultBefore < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

                int resultCreate;

                try
                {
                    resultCreate = await _recordRepository.Create(opportunity, opportunityModule);

                    // Create stage history
                    await _recordHelper.CreateStageHistory(opportunity);
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

	            _recordHelper.AfterCreate(opportunityModule, opportunity, AppUser, _warehouse);
            }

            // Update lead as converted
            lead["is_converted"] = true;
            var convertedField = leadModule.Fields.FirstOrDefault(x => x.Name == "converted" && !x.Deleted);

            if (convertedField != null)
                lead["converted"] = true;

            if ((bool)request["deleted"])
                lead["deleted"] = true;
            var leadModel = new JObject();
            int resultUpdate;

            foreach (var pair in lead)
            {
                if (!pair.Value.IsNullOrEmpty())
                    leadModel[pair.Key] = lead[pair.Key];
            }

            try
            {
                resultUpdate = await _recordRepository.Update(leadModel, leadModule, (bool)request["deleted"]);
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

	        _recordHelper.AfterDelete(leadModule, leadModel, AppUser, _warehouse);

            // Move documents
            var documents = await _documentRepository.GetAll(leadModule.Id, (int)lead["id"]);

            foreach (var document in documents)
            {
                document.ModuleId = accountModule.Id;
                document.RecordId = (int)account["id"];

                await _documentRepository.Update(document);
            }

            // Move activities
            var findRequest = new FindRequest
            {
                Fields = new List<string> { "related_to" },
                Filters = new List<Filter> { new Filter { Field = "related_to", Operator = Operator.Equals, Value = (int)lead["id"], No = 1 } },
                Limit = 1000,
                Offset = 0
            };

            var activities = _recordRepository.Find("activities", findRequest, false);

            foreach (JObject activity in activities)
            {
                activity["related_to"] = (int)account["id"];
                activity["related_module"] = AppUser.TenantLanguage == "tr" ? accountModule.LabelTrSingular : accountModule.LabelEnSingular;
                await _recordRepository.Update(activity, activityModule);
            }

            // Move notes
            var noteRequest = new NoteRequest
            {
                ModuleId = leadModule.Id,
                RecordId = (int)lead["id"],
                Limit = 1000,
                Offset = 0
            };

            var notes = await _noteRepository.Find(noteRequest);

            foreach (var note in notes)
            {
                note.ModuleId = accountModule.Id;
                note.RecordId = (int)account["id"];
                await _noteRepository.Update(note);
            }

            var result = new JObject();
            result["account_id"] = account["id"];
            result["contact_id"] = contact["id"];

            if (opportunity != null)
                result["opportunity_id"] = opportunity["id"];

            return Ok(result);
        }

        [Route("convert_quote")]
        [HttpPost]
        public async Task<dynamic> ConvertQuote([FromBody]JObject request)
        {
            var quoteModule = await _moduleRepository.GetByName("quotes");
            var salesOrderModule = await _moduleRepository.GetByName("sales_orders");
            var orderProductModule = await _moduleRepository.GetByName("order_products");
            var quote = _recordRepository.GetById(quoteModule, (int)request["quote_id"], false);
            var conversionMappings = await _conversionMappingRepository.GetAll(quoteModule.Id);

            // Prepare order
            var salesOrder = new JObject();
            salesOrder["owner"] = quote["owner"];
            salesOrder["total"] = quote["total"];
            salesOrder["discounted_total"] = quote["discounted_total"];
            salesOrder["vat_total"] = quote["vat_total"];
            salesOrder["grand_total"] = quote["grand_total"];
            salesOrder["discount_amount"] = quote["discount_amount"];
            salesOrder["discount_percent"] = quote["discount_percent"];
            salesOrder["discount_type"] = quote["discount_type"];
            salesOrder["vat_list"] = quote["vat_list"];
            salesOrder["quote"] = quote["id"];
            salesOrder["approved"] = false;

            if (!salesOrder["currency"].IsNullOrEmpty() && !quote["currency"].IsNullOrEmpty())
                salesOrder["currency"] = quote["currency"];

            if (request["order_stage"] != null)
                salesOrder["order_stage"] = request["order_stage"];

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            if (conversionMappings == null || conversionMappings.Count < 1)
                return BadRequest("Conversion mappings not found.");

            foreach (var conversionMapping in conversionMappings)
            {
                var field = quoteModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.FieldId);
                var mappingField = conversionMapping.MappingModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.MappingFieldId);

                if (mappingField != null && mappingField.Name == "order_stage" && request["order_stage"] != null)
                    continue;

                if (field != null && mappingField != null)
                    salesOrder[mappingField.Name] = quote[field.Name];
            }

            // Create sales order
            if (!salesOrder["currency"].IsNullOrEmpty())
            {
                var currencyfield = quoteModule.Fields.FirstOrDefault(x => x.Name == "currency");
                var currencyPicklist = await _picklistRepository.FindItemByLabel(currencyfield.PicklistId.Value, (string)salesOrder["currency"], AppUser.TenantLanguage);
                salesOrder["currency"] = AppUser.TenantLanguage == "tr" ? currencyPicklist.LabelTr : currencyPicklist.LabelEn;
            }
            var resultBefore = await _recordHelper.BeforeCreateUpdate(salesOrderModule, salesOrder, ModelState, AppUser.TenantLanguage, false);

            if (resultBefore < 0 && !ModelState.IsValid)
                return BadRequest(ModelState);

            int resultCreate;

            try
            {
                resultCreate = await _recordRepository.Create(salesOrder, salesOrderModule);
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

	        _recordHelper.AfterCreate(salesOrderModule, salesOrder, AppUser, _warehouse);

            // Get all quote products and insert order products
            var findRequest = new FindRequest
            {
                Filters = new List<Filter> { new Filter { Field = "quote", Operator = Operator.Equals, Value = (int)quote["id"], No = 1 } },
                Limit = 1000,
                Offset = 0
            };

            var quoteProducts = _recordRepository.Find("quote_products", findRequest, false);

            foreach (var quoteProduct in quoteProducts)
            {
                var orderProduct = new JObject();
                orderProduct["sales_order"] = salesOrder["id"];
                orderProduct["order"] = quoteProduct["order"];
                orderProduct["amount"] = quoteProduct["amount"];
                orderProduct["product"] = quoteProduct["product"];
                orderProduct["quantity"] = quoteProduct["quantity"];
                orderProduct["unit_price"] = quoteProduct["unit_price"];
                orderProduct["usage_unit"] = quoteProduct["usage_unit"];

                if (quoteProduct["discount_amount"] != null)
                    orderProduct["discount_amount"] = quoteProduct["discount_amount"];

                if (quoteProduct["currency"] != null)
                    orderProduct["currency"] = quoteProduct["currency"];

                if (quoteProduct["vat_percent"] != null)
                    orderProduct["vat_percent"] = quoteProduct["vat_percent"];

                if (quoteProduct["discount_type"] != null)
                    orderProduct["discount_type"] = quoteProduct["discount_type"];

                if (quoteProduct["vat"] != null)
                    orderProduct["vat"] = quoteProduct["vat"];

                if (quoteProduct["discount_percent"] != null)
                    orderProduct["discount_percent"] = quoteProduct["discount_percent"];

                if (quoteProduct["separator"] != null)
                    orderProduct["separator"] = quoteProduct["separator"];

                var resultBeforeProduct = await _recordHelper.BeforeCreateUpdate(orderProductModule, orderProduct, ModelState, AppUser.TenantLanguage, false);

                if (resultBeforeProduct < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

                int resultCreateProduct;

                try
                {
                    resultCreateProduct = await _recordRepository.Create(orderProduct, orderProductModule);
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

                if (resultCreateProduct < 1)
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
				//throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

	            _recordHelper.AfterCreate(orderProductModule, orderProduct, AppUser, _warehouse);
            }

            // Update quote as converted
            var convertedStagePicklist = await _picklistRepository.GetItemBySystemCode("converted_quote_stage");
            quote["quote_stage"] = AppUser.TenantLanguage == "tr" ? convertedStagePicklist.LabelTr : convertedStagePicklist.LabelEn;
            quote["is_converted"] = true;
            var quoteModel = new JObject();
            int resultUpdate;

            foreach (var pair in quote)
            {
                if (!pair.Value.IsNullOrEmpty())
                    quoteModel[pair.Key] = quote[pair.Key];
            }

            try
            {
                resultUpdate = await _recordRepository.Update(quoteModel, quoteModule);
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

	        _recordHelper.AfterUpdate(quoteModule, quoteModel, quote, AppUser, _warehouse);

            var result = new JObject();
            result["sales_order_id"] = salesOrder["id"];

            return Ok(result);
        }


        [Route("convert_candidate")]
        [HttpPost]
        public async Task<IActionResult> ConvertPersonal([FromBody]JObject request)
        {
            var candidateModule = await _moduleRepository.GetByName("adaylar");
            var employeeModule = await _moduleRepository.GetByName("calisanlar");
            var activityModule = await _moduleRepository.GetByName("activities");
            var candidate = _recordRepository.GetById(candidateModule, (int)request["lead_id"], false);
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
                var resultBefore = await _recordHelper.BeforeCreateUpdate(employeeModule, employee, ModelState, AppUser.TenantLanguage, false);

                if (resultBefore < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

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

            var activities = _recordRepository.Find("activities", findRequestActivity, false);

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

                var subModuleRecords = _recordRepository.Find(conversion.MappingSubModule.Name, findRequest, false);

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
        [Route("convert_sales_orders")]
        [HttpPost]
        public async Task<IActionResult> ConvertSalesOrders([FromBody]JObject request)
        {
            var salesOrderModule = await _moduleRepository.GetByName("sales_orders");
            var salesInvoiceModule = await _moduleRepository.GetByName("sales_invoices");
            var activityModule = await _moduleRepository.GetByName("activities");
            var salesOrder = _recordRepository.GetById(salesOrderModule, (int)request["sales_order_id"], false);
            var conversionMappings = await _conversionMappingRepository.GetAll(salesOrderModule.Id);
            var salesInvoicesProductModule = await _moduleRepository.GetByName("sales_invoices_products");
            var salesInvoice = new JObject();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            if (conversionMappings == null || conversionMappings.Count < 1)
                return BadRequest("Conversion mappings not found.");

            // Prepare new records
            foreach (var conversionMapping in conversionMappings)
            {
                var field = salesOrderModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.FieldId);
                var mappingField = conversionMapping.MappingModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.MappingFieldId);

                switch (conversionMapping.MappingModule.Name)
                {
                    case "sales_invoices":
                        if (field != null && mappingField != null && !salesOrder[field.Name].IsNullOrEmpty())
                            salesInvoice[mappingField.Name] = salesOrder[field.Name];
                        break;
                }
            }

            // Create new records
            // account
            if (salesInvoice.HasValues)
            {
                salesInvoice["owner"] = salesOrder["owner"];
                salesInvoice["master_id"] = salesOrder["id"];
                salesInvoice["siparis"] = salesOrder["id"];
                salesInvoice["fatura_tarihi"] = DateTime.UtcNow;
                salesInvoice["vade_tarihi"] = DateTime.UtcNow;

                salesInvoice["total"] = salesOrder["total"];
                salesInvoice["discounted_total"] = salesOrder["discounted_total"];
                salesInvoice["vat_total"] = salesOrder["vat_total"];
                salesInvoice["grand_total"] = salesOrder["grand_total"];
                salesInvoice["discount_amount"] = salesOrder["discount_amount"];
                salesInvoice["discount_percent"] = salesOrder["discount_percent"];
                salesInvoice["discount_type"] = salesOrder["discount_type"];
                salesInvoice["vat_list"] = salesOrder["vat_list"];

                if (!salesOrder["currency"].IsNullOrEmpty())
                    salesInvoice["currency"] = salesOrder["currency"];


                var asamaField = salesInvoiceModule.Fields.Single(x => x.Name == "asama");
                var asamaTypes = await _picklistRepository.GetById(asamaField.PicklistId.Value);
                salesInvoice["asama"] = AppUser.TenantLanguage == "tr" ? asamaTypes.Items.Single(x => x.SystemCode == "onaylandi").LabelTr : asamaTypes.Items.Single(x => x.SystemCode == "onaylandi").LabelEn;

                var resultBefore = await _recordHelper.BeforeCreateUpdate(salesInvoiceModule, salesInvoice, ModelState, AppUser.TenantLanguage, false);

                if (resultBefore < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

                int resultCreate;

                try
                {
                    resultCreate = await _recordRepository.Create(salesInvoice, salesInvoiceModule);
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

	            _recordHelper.AfterCreate(salesInvoiceModule, salesInvoice, AppUser, _warehouse);

                // Get all quote products and insert order products
                var findRequest = new FindRequest
                {
                    Filters = new List<Filter> { new Filter { Field = "sales_order", Operator = Operator.Equals, Value = (int)salesOrder["id"], No = 1 } },
                    Limit = 1000,
                    Offset = 0
                };

                var orderProducts = _recordRepository.Find("order_products", findRequest, false);

                foreach (var orderProduct in orderProducts)
                {
                    var salesInvoicesProduct = new JObject();
                    salesInvoicesProduct["sales_invoice"] = salesInvoice["id"];
                    salesInvoicesProduct["order"] = orderProduct["order"];
                    salesInvoicesProduct["amount"] = orderProduct["amount"];
                    salesInvoicesProduct["product"] = orderProduct["product"];
                    salesInvoicesProduct["quantity"] = orderProduct["quantity"];
                    salesInvoicesProduct["unit_price"] = orderProduct["unit_price"];
                    salesInvoicesProduct["usage_unit"] = orderProduct["usage_unit"];

                    if (orderProduct["discount_amount"] != null)
                        salesInvoicesProduct["discount_amount"] = orderProduct["discount_amount"];

                    if (orderProduct["currency"] != null)
                        salesInvoicesProduct["currency"] = orderProduct["currency"];

                    if (orderProduct["vat_percent"] != null)
                        salesInvoicesProduct["vat_percent"] = orderProduct["vat_percent"];

                    if (orderProduct["discount_type"] != null)
                        salesInvoicesProduct["discount_type"] = orderProduct["discount_type"];

                    if (orderProduct["vat"] != null)
                        salesInvoicesProduct["vat"] = orderProduct["vat"];

                    if (orderProduct["discount_percent"] != null)
                        salesInvoicesProduct["discount_percent"] = orderProduct["discount_percent"];

                    if (orderProduct["separator"] != null)
                        salesInvoicesProduct["separator"] = orderProduct["separator"];

                    var resultBeforeProduct = await _recordHelper.BeforeCreateUpdate(salesInvoicesProductModule, salesInvoicesProduct, ModelState, AppUser.TenantLanguage, false);

                    if (resultBeforeProduct < 0 && !ModelState.IsValid)
                        return BadRequest(ModelState);

                    int resultCreateProduct;

                    try
                    {
                        resultCreateProduct = await _recordRepository.Create(salesInvoicesProduct, salesInvoicesProductModule);
                    }
                    catch (PostgresException ex)
                    {
                        if (ex.SqlState == PostgreSqlStateCodes.UniqueViolation)
                            return StatusCode(HttpStatusCode.Status409Conflict, new { message = ex.MessageText });
                   

                        if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                            return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                        if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                            return StatusCode(HttpStatusCode.Status400BadRequest, new { message = ex.MessageText });

                        throw;
                    }

                    if (resultCreateProduct < 1)
                        throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

                    _recordHelper.AfterCreate(salesInvoicesProductModule, salesInvoicesProduct, AppUser, _warehouse);
                }
            }

            //Update order stage
            var orderStageTypeField = salesOrderModule.Fields.Single(x => x.Name == "order_stage");
            var orderStageTypes = await _picklistRepository.GetById(orderStageTypeField.PicklistId.Value);
            salesOrder["order_stage"] = AppUser.TenantLanguage == "tr" ? orderStageTypes.Items.Single(x => x.SystemCode == "converted_to_sales_invoice").LabelTr : orderStageTypes.Items.Single(x => x.SystemCode == "converted_to_sales_invoice").LabelEn;

            // Update lead as converted
            salesOrder["is_converted"] = true;

            var convertedField = salesOrderModule.Fields.FirstOrDefault(x => x.Name == "converted" && !x.Deleted);

            if (convertedField != null)
                salesOrder["converted"] = true;

            if ((bool)request["deleted"] == true)
                salesOrder["deleted"] = true;

            var leadModel = new JObject();
            int resultUpdate;

            foreach (var pair in salesOrder)
            {
                if (!pair.Value.IsNullOrEmpty())
                    leadModel[pair.Key] = salesOrder[pair.Key];
            }

            try
            {
                resultUpdate = await _recordRepository.Update(leadModel, salesOrderModule, (bool)request["deleted"]);
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

	        _recordHelper.AfterDelete(salesOrderModule, leadModel, AppUser, _warehouse);

            // Move documents
            var documents = await _documentRepository.GetAll(salesOrderModule.Id, (int)salesOrder["id"]);

            foreach (var document in documents)
            {
                document.ModuleId = salesInvoiceModule.Id;
                document.RecordId = (int)salesInvoice["id"];

                await _documentRepository.Update(document);
            }

            // Move activities
            var findRequestActivity = new FindRequest
            {
                Fields = new List<string> { "related_to" },
                Filters = new List<Filter> { new Filter { Field = "related_to", Operator = Operator.Equals, Value = (int)salesOrder["id"], No = 1 } },
                Limit = 1000,
                Offset = 0
            };

            var activities = _recordRepository.Find("activities", findRequestActivity, false);

            foreach (JObject activity in activities)
            {
                activity["related_to"] = (int)salesInvoice["id"];
                await _recordRepository.Update(activity, activityModule);
            }

            // Move notes
            var noteRequest = new NoteRequest
            {
                ModuleId = salesOrderModule.Id,
                RecordId = (int)salesOrder["id"],
                Limit = 1000,
                Offset = 0
            };

            var notes = await _noteRepository.Find(noteRequest);

            foreach (var note in notes)
            {
                note.ModuleId = salesInvoiceModule.Id;
                note.RecordId = (int)salesInvoice["id"];
                await _noteRepository.Update(note);
            }

            //Move dynamic sub modules
            var conversionsubModules = await _conversionMappingRepository.GetSubConversions(salesOrderModule.Id);
            foreach (var conversion in conversionsubModules)
            {
                var subModule = await _moduleRepository.GetByName(conversion.MappingSubModule.Name);
                var findRequest = new FindRequest
                {
                    Filters = new List<Filter> { new Filter { Field = conversion.SubModuleSourceField, Operator = Operator.Equals, Value = (int)salesOrder["id"], No = 1 } },
                    Limit = 1000,
                    Offset = 0
                };

                var subModuleRecords = _recordRepository.Find(conversion.MappingSubModule.Name, findRequest, false);

                foreach (JObject subRecord in subModuleRecords)
                {
                    subRecord[conversion.SubModuleDestinationField] = (int)salesInvoice["id"];
                    await _recordRepository.Update(subRecord, conversion.MappingSubModule);
                }
            }


            var result = new JObject();
            result["sales_invoice_id"] = salesInvoice["id"];

            return Ok(result);
        }


        [Route("convert_purchase_orders")]
        [HttpPost]
        public async Task<IActionResult> PurchaseSalesOrders(JObject request)
        {
            var purchaseOrderModule = await _moduleRepository.GetByName("purchase_orders");
            var purchaseInvoiceModule = await _moduleRepository.GetByName("purchase_invoices");
            var activityModule = await _moduleRepository.GetByName("activities");
            var purchaseOrder = _recordRepository.GetById(purchaseOrderModule, (int)request["purchase_order_id"], false);
            var purchaseInvoiceProductModule = await _moduleRepository.GetByName("purchase_invoices_products");
            var conversionMappings = await _conversionMappingRepository.GetAll(purchaseOrderModule.Id);
            var purchaseInvoice = new JObject();

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            if (conversionMappings == null || conversionMappings.Count < 1)
                return BadRequest("Conversion mappings not found.");

            // Prepare new records
            foreach (var conversionMapping in conversionMappings)
            {
                var field = purchaseOrderModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.FieldId);
                var mappingField = conversionMapping.MappingModule.Fields.FirstOrDefault(x => x.Id == conversionMapping.MappingFieldId);

                switch (conversionMapping.MappingModule.Name)
                {
                    case "purchase_invoices":
                        if (field != null && mappingField != null && !purchaseOrder[field.Name].IsNullOrEmpty())
                            purchaseInvoice[mappingField.Name] = purchaseOrder[field.Name];
                        break;
                }
            }

            // Create new records
            // account
            if (purchaseInvoice.HasValues)
            {
                purchaseInvoice["owner"] = purchaseOrder["owner"];
                purchaseInvoice["master_id"] = purchaseOrder["id"];
                purchaseInvoice["tedarikci_siparis"] = purchaseOrder["id"];
                purchaseInvoice["fatura_tarihi"] = DateTime.UtcNow;
                purchaseInvoice["vade_tarihi"] = DateTime.UtcNow;

                var asamaField = purchaseInvoiceModule.Fields.Single(x => x.Name == "asama");
                var asamaTypes = await _picklistRepository.GetById(asamaField.PicklistId.Value);
                purchaseInvoice["asama"] = AppUser.TenantLanguage == "tr" ? asamaTypes.Items.Single(x => x.SystemCode == "onaylandi").LabelTr : asamaTypes.Items.Single(x => x.SystemCode == "onaylandi").LabelEn;

                var resultBefore = await _recordHelper.BeforeCreateUpdate(purchaseInvoiceModule, purchaseInvoice, ModelState, AppUser.TenantLanguage, false);

                if (resultBefore < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

                int resultCreate;

                try
                {
                    resultCreate = await _recordRepository.Create(purchaseInvoice, purchaseInvoiceModule);
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

	            _recordHelper.AfterCreate(purchaseInvoiceModule, purchaseInvoice, AppUser, _warehouse);

                // Get all quote products and insert order products
                var findRequest = new FindRequest
                {
                    Filters = new List<Filter> { new Filter { Field = "purchase_order", Operator = Operator.Equals, Value = (int)purchaseOrder["id"], No = 1 } },
                    Limit = 1000,
                    Offset = 0
                };

                var purchaseOrderProducts = _recordRepository.Find("purchase_order_products", findRequest, false);

                foreach (var purchaseOrderProduct in purchaseOrderProducts)
                {
                    var purchaseInvoicesProduct = new JObject();
                    purchaseInvoicesProduct["purchase_invoice"] = purchaseInvoice["id"];
                    purchaseInvoicesProduct["order"] = purchaseOrderProduct["order"];
                    purchaseInvoicesProduct["amount"] = purchaseOrderProduct["amount"];
                    purchaseInvoicesProduct["product"] = purchaseOrderProduct["product"];
                    purchaseInvoicesProduct["quantity"] = purchaseOrderProduct["quantity"];
                    purchaseInvoicesProduct["unit_price"] = purchaseOrderProduct["unit_price"];
                    purchaseInvoicesProduct["usage_unit"] = purchaseOrderProduct["usage_unit"];
                    purchaseInvoicesProduct["purchase_price"] = purchaseOrderProduct["purchase_price"];

                    if (purchaseOrderProduct["discount_amount"] != null)
                        purchaseInvoicesProduct["discount_amount"] = purchaseOrderProduct["discount_amount"];

                    if (purchaseOrderProduct["currency"] != null)
                        purchaseInvoicesProduct["currency"] = purchaseOrderProduct["currency"];

                    if (purchaseOrderProduct["vat_percent"] != null)
                        purchaseInvoicesProduct["vat_percent"] = purchaseOrderProduct["vat_percent"];

                    if (purchaseOrderProduct["discount_type"] != null)
                        purchaseInvoicesProduct["discount_type"] = purchaseOrderProduct["discount_type"];

                    if (purchaseOrderProduct["vat"] != null)
                        purchaseInvoicesProduct["vat"] = purchaseOrderProduct["vat"];

                    if (purchaseOrderProduct["discount_percent"] != null)
                        purchaseInvoicesProduct["discount_percent"] = purchaseOrderProduct["discount_percent"];

                    if (purchaseOrderProduct["separator"] != null)
                        purchaseInvoicesProduct["separator"] = purchaseOrderProduct["separator"];

                    var resultBeforeProduct = await _recordHelper.BeforeCreateUpdate(purchaseInvoiceProductModule, purchaseInvoicesProduct, ModelState, AppUser.TenantLanguage, false);

                    if (resultBeforeProduct < 0 && !ModelState.IsValid)
                        return BadRequest(ModelState);

                    int resultCreateProduct;

                    try
                    {
                        resultCreateProduct = await _recordRepository.Create(purchaseInvoicesProduct, purchaseInvoiceProductModule);
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

                    _recordHelper.AfterCreate(purchaseInvoiceProductModule, purchaseInvoicesProduct, AppUser, _warehouse);
                }
            }

            //Update order stage
            var orderStageTypeField = purchaseOrderModule.Fields.Single(x => x.Name == "order_stage");
            var orderStageTypes = await _picklistRepository.GetById(orderStageTypeField.PicklistId.Value);
            purchaseOrder["order_stage"] = AppUser.TenantLanguage == "tr" ? orderStageTypes.Items.Single(x => x.SystemCode == "converted_to_purchase_invoice").LabelTr : orderStageTypes.Items.Single(x => x.SystemCode == "converted_to_purchase_invoice").LabelEn;

            // Update lead as converted
            purchaseOrder["is_converted"] = true;

            var convertedField = purchaseOrderModule.Fields.FirstOrDefault(x => x.Name == "converted" && !x.Deleted);

            if (convertedField != null)
                purchaseOrder["converted"] = true;

            if ((bool)request["deleted"] == true)
                purchaseOrder["deleted"] = true;

            var leadModel = new JObject();
            int resultUpdate;

            foreach (var pair in purchaseOrder)
            {
                if (!pair.Value.IsNullOrEmpty())
                    leadModel[pair.Key] = purchaseOrder[pair.Key];
            }

            try
            {
                resultUpdate = await _recordRepository.Update(leadModel, purchaseOrderModule, (bool)request["deleted"]);
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

	        _recordHelper.AfterDelete(purchaseOrderModule, leadModel, AppUser, _warehouse);

            // Move documents
            var documents = await _documentRepository.GetAll(purchaseOrderModule.Id, (int)purchaseOrder["id"]);

            foreach (var document in documents)
            {
                document.ModuleId = purchaseInvoiceModule.Id;
                document.RecordId = (int)purchaseInvoice["id"];

                await _documentRepository.Update(document);
            }

            // Move activities
            var findRequestActivity = new FindRequest
            {
                Fields = new List<string> { "related_to" },
                Filters = new List<Filter> { new Filter { Field = "related_to", Operator = Operator.Equals, Value = (int)purchaseOrder["id"], No = 1 } },
                Limit = 1000,
                Offset = 0
            };

            var activities = _recordRepository.Find("activities", findRequestActivity, false);

            foreach (JObject activity in activities)
            {
                activity["related_to"] = (int)purchaseInvoice["id"];
                await _recordRepository.Update(activity, activityModule);
            }

            // Move notes
            var noteRequest = new NoteRequest
            {
                ModuleId = purchaseOrderModule.Id,
                RecordId = (int)purchaseOrder["id"],
                Limit = 1000,
                Offset = 0
            };

            var notes = await _noteRepository.Find(noteRequest);

            foreach (var note in notes)
            {
                note.ModuleId = purchaseInvoiceModule.Id;
                note.RecordId = (int)purchaseInvoice["id"];
                await _noteRepository.Update(note);
            }

            //Move dynamic sub modules
            var conversionsubModules = await _conversionMappingRepository.GetSubConversions(purchaseOrderModule.Id);
            foreach (var conversion in conversionsubModules)
            {
                var subModule = await _moduleRepository.GetByName(conversion.MappingSubModule.Name);
                var findRequest = new FindRequest
                {
                    Filters = new List<Filter> { new Filter { Field = conversion.SubModuleSourceField, Operator = Operator.Equals, Value = (int)purchaseOrder["id"], No = 1 } },
                    Limit = 1000,
                    Offset = 0
                };

                var subModuleRecords = _recordRepository.Find(conversion.MappingSubModule.Name, findRequest, false);

                foreach (JObject subRecord in subModuleRecords)
                {
                    subRecord[conversion.SubModuleDestinationField] = (int)purchaseInvoice["id"];
                    await _recordRepository.Update(subRecord, conversion.MappingSubModule);
                }
            }


            var result = new JObject();
            result["purchase_invoice_id"] = purchaseInvoice["id"];

            return Ok(result);
        }
    }
}