using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Npgsql;
using PrimeApps.App.ActionFilters;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using RecordHelper = PrimeApps.App.Helpers.RecordHelper;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers.QueryTranslation;

namespace PrimeApps.App.Controllers
{
    [Route("api/convert"), Authorize, SnakeCase]
    public class ConvertController : BaseController
    {
        private IModuleRepository _moduleRepository;
        private IRecordRepository _recordRepository;
        private IPicklistRepository _picklistRepository;
        private IDocumentRepository _documentRepository;
        private INoteRepository _noteRepository;
        private IConversionMappingRepository _conversionMappingRepository;
        private Warehouse _warehouse;

        public ConvertController(IModuleRepository moduleRepository, IRecordRepository recordRepository, IPicklistRepository picklistRepository, IDocumentRepository documentRepository, INoteRepository noteRepository, IConversionMappingRepository conversionMappingRepository, Warehouse warehouse)
        {
            _moduleRepository = moduleRepository;
            _recordRepository = recordRepository;
            _picklistRepository = picklistRepository;
            _documentRepository = documentRepository;
            _noteRepository = noteRepository;
            _conversionMappingRepository = conversionMappingRepository;
            _warehouse = warehouse;
        }

        [Route("create_mapping"), HttpPost]
        public async Task<IActionResult> CreateLeadConversionMapping(ConversionMapping conversionMapping)
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
        public async Task<IActionResult> Delete(ConversionMapping conversionMapping)
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
        public async Task<IActionResult> DeleteFieldsMappings(List<int> fieldsId)
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
        public async Task<IActionResult> ConvertLead(JObject request)
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
                var resultBefore = await RecordHelper.BeforeCreateUpdate(accountModule, account, ModelState, AppUser.TenantLanguage, _moduleRepository, _picklistRepository, false);

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
                        return Content(HttpStatusCode.Conflict, RecordHelper.PrepareConflictError(ex));

                    if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                        return Content(HttpStatusCode.BadRequest, new { message = ex.Detail });

                    if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                        return Content(HttpStatusCode.BadRequest, new { message = ex.MessageText });

                    throw;
                }

                if (resultCreate < 1)
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);

                RecordHelper.AfterCreate(accountModule, account, AppUser, _warehouse);
            }

            // contact
            if (contact.HasValues)
            {
                contact["owner"] = lead["owner"];
                contact["master_id"] = lead["id"];

                if (account["id"] != null && contactModule.Fields.Any(x => x.Name == "account"))
                    contact["account"] = account["id"];

                var resultBefore = await RecordHelper.BeforeCreateUpdate(contactModule, contact, ModelState, AppUser.TenantLanguage, _moduleRepository, _picklistRepository, false);

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
                        return Content(HttpStatusCode.Conflict, RecordHelper.PrepareConflictError(ex));

                    if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                        return Content(HttpStatusCode.BadRequest, new { message = ex.Detail });

                    if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                        return Content(HttpStatusCode.BadRequest, new { message = ex.MessageText });

                    throw;
                }

                if (resultCreate < 1)
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);

                RecordHelper.AfterCreate(contactModule, contact, AppUser, _warehouse);
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

                var resultBefore = await RecordHelper.BeforeCreateUpdate(opportunityModule, opportunity, ModelState, AppUser.TenantLanguage, _moduleRepository, _picklistRepository);

                if (resultBefore < 0 && !ModelState.IsValid)
                    return BadRequest(ModelState);

                int resultCreate;

                try
                {
                    resultCreate = await _recordRepository.Create(opportunity, opportunityModule);

                    // Create stage history
                    await RecordHelper.CreateStageHistory(opportunity, _recordRepository, _moduleRepository);
                }
                catch (PostgresException ex)
                {
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

                RecordHelper.AfterCreate(opportunityModule, opportunity, AppUser, _warehouse);
            }

            // Update lead as converted
            lead["is_converted"] = true;
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
                resultUpdate = await _recordRepository.Update(leadModel, leadModule, true);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.UniqueViolation)
                    return Content(HttpStatusCode.Conflict, RecordHelper.PrepareConflictError(ex));

                if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.Detail });

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.MessageText });

                throw;
            }

            if (resultUpdate < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            RecordHelper.AfterDelete(leadModule, leadModel, AppUser, _warehouse);

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
                activity["related_to"] = (int)contact["id"];
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
        public async Task<dynamic> ConvertQuote(JObject request)
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
                salesOrder["currency"] = currencyPicklist.Id;
            }
            var resultBefore = await RecordHelper.BeforeCreateUpdate(salesOrderModule, salesOrder, ModelState, AppUser.TenantLanguage, _moduleRepository, _picklistRepository, false);

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
                    return Content(HttpStatusCode.Conflict, RecordHelper.PrepareConflictError(ex));

                if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.Detail });

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.MessageText });

                throw;
            }

            if (resultCreate < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            RecordHelper.AfterCreate(salesOrderModule, salesOrder, AppUser, _warehouse);

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

                if (quoteProduct["discount_type"] != null)
                    orderProduct["discount_type"] = quoteProduct["discount_type"];

                if (quoteProduct["vat"] != null)
                    orderProduct["vat"] = quoteProduct["vat"];

                var resultBeforeProduct = await RecordHelper.BeforeCreateUpdate(orderProductModule, orderProduct, ModelState, AppUser.TenantLanguage, _moduleRepository, _picklistRepository, false);

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
                        return Content(HttpStatusCode.Conflict, RecordHelper.PrepareConflictError(ex));

                    if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                        return Content(HttpStatusCode.BadRequest, new { message = ex.Detail });

                    if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                        return Content(HttpStatusCode.BadRequest, new { message = ex.MessageText });

                    throw;
                }

                if (resultCreateProduct < 1)
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);

                RecordHelper.AfterCreate(orderProductModule, orderProduct, AppUser, _warehouse);
            }

            // Update quote as converted
            var convertedStagePicklist = await _picklistRepository.GetItemBySystemCode("converted_quote_stage");
            quote["quote_stage"] = AppUser.TenantLanguage == "tr" ? convertedStagePicklist.LabelTr : convertedStagePicklist.LabelEn;
            quote["is_converted"] = true;
            quote["deleted"] = true;
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
                    return Content(HttpStatusCode.Conflict, RecordHelper.PrepareConflictError(ex));

                if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.Detail });

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.MessageText });

                throw;
            }

            if (resultUpdate < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            RecordHelper.AfterUpdate(quoteModule, quoteModel, quote, AppUser, _warehouse);

            var result = new JObject();
            result["sales_order_id"] = salesOrder["id"];

            return Ok(result);
        }


        [Route("convert_candidate")]
        [HttpPost]
        public async Task<IHttpActionResult> ConvertPersonal(JObject request)
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
                var resultBefore = await RecordHelper.BeforeCreateUpdate(employeeModule, employee, ModelState, AppUser.TenantLanguage, _moduleRepository, _picklistRepository, false);

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
                        return Content(HttpStatusCode.Conflict, RecordHelper.PrepareConflictError(ex));

                    if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                        return Content(HttpStatusCode.BadRequest, new { message = ex.Detail });

                    if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                        return Content(HttpStatusCode.BadRequest, new { message = ex.MessageText });

                    throw;
                }

                if (resultCreate < 1)
                    throw new HttpResponseException(HttpStatusCode.InternalServerError);

                RecordHelper.AfterCreate(employeeModule, employee, AppUser, _warehouse);
            }

            // Update lead as converted
            candidate["is_converted"] = true;

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
                    return Content(HttpStatusCode.Conflict, RecordHelper.PrepareConflictError(ex));

                if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.Detail });

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return Content(HttpStatusCode.BadRequest, new { message = ex.MessageText });

                throw;
            }

            if (resultUpdate < 1)
                throw new HttpResponseException(HttpStatusCode.InternalServerError);

            RecordHelper.AfterDelete(candidateModule, leadModel, AppUser, _warehouse);

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
    }
}