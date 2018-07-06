using Aspose.Cells;
using Aspose.Words;
using Aspose.Words.MailMerging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using MimeMapping;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.App.Extensions;
using PrimeApps.App.Helpers;
using PrimeApps.App.Storage;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Helpers.QueryTranslation;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static PrimeApps.App.Controllers.DocumentController;

namespace PrimeApps.App.Controllers
{
    [Route("attach")]
    public class AttachController : MvcBaseController
    {

        private ITenantRepository _tenantRepository;
        private ITemplateRepository _templateRepository;
        private IModuleRepository _moduleRepository;
        private IRecordRepository _recordRepository;
        private IPicklistRepository _picklistRepository;
        private ISettingRepository _settingsRepository;
        private INoteRepository _noteRepository;
        private IConfiguration _configuration;
        private IDocumentRepository _documentRepository;

        private IRecordHelper _recordHelper;
        public AttachController(ITenantRepository tenantRepository, IDocumentRepository documentRepository, IModuleRepository moduleRepository, IRecordRepository recordRepository, ITemplateRepository templateRepository, IPicklistRepository picklistRepository, ISettingRepository settingsRepository, IRecordHelper recordHelper, INoteRepository noteRepository, IConfiguration configuration, IHostingEnvironment hostingEnvironment, IUnifiedStorage unifiedStorage)
        {
            _tenantRepository = tenantRepository;
            _documentRepository = documentRepository;
            _moduleRepository = moduleRepository;
            _recordRepository = recordRepository;
            _templateRepository = templateRepository;
            _picklistRepository = picklistRepository;
            _settingsRepository = settingsRepository;
            _noteRepository = noteRepository;
            _recordHelper = recordHelper;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_moduleRepository);
            SetCurrentUser(_recordRepository);
            SetCurrentUser(_tenantRepository);
            SetCurrentUser(_templateRepository);
            SetCurrentUser(_documentRepository);
            SetCurrentUser(_picklistRepository);
            SetCurrentUser(_settingsRepository);
            SetCurrentUser(_noteRepository);
            SetCurrentUser(_moduleRepository);
            _recordHelper.SetCurrentUser(AppUser);
            base.OnActionExecuting(context);
        }

        [Route("export")]
        public async Task<IActionResult> Export([FromQuery(Name = "module")]string module, [FromQuery(Name = "id")]int id, [FromQuery(Name = "templateId")]int templateId, [FromQuery(Name = "format")]string format, [FromQuery(Name = "locale")]string locale, [FromQuery(Name = "timezoneOffset")]int timezoneOffset = 180, [FromQuery(Name = "save")] bool save = false)
        {
            JObject record;
            var relatedModuleRecords = new Dictionary<string, JArray>();
            var notes = new List<Note>();
            var moduleEntity = await _moduleRepository.GetByName(module);
            var currentCulture = locale == "en" ? "en-US" : "tr-TR";


            if (moduleEntity == null)
            {
                return BadRequest();
            }

            var templateEntity = await _templateRepository.GetById(templateId);

            if (templateEntity == null)
            {
                return BadRequest();
            }

            //if there is a template with this id, try to get it from blob AzureStorage.
            var templateBlob = AzureStorage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), $"templates/{templateEntity.Content}", _configuration);

            try
            {
                //try to get the attributes of blob.
                await templateBlob.FetchAttributesAsync();
            }
            catch (Exception)
            {
                //if there is an exception, it means there is no such file.
                return NotFound();
            }

            if (module == "users")
            {
                moduleEntity = Model.Helpers.ModuleHelper.GetFakeUserModule();
            }

            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository);

            try
            {
                record = _recordRepository.GetById(moduleEntity, id, !AppUser.HasAdminProfile, lookupModules);

                if (record == null)
                {
                    return NotFound();
                }

                foreach (var field in moduleEntity.Fields)
                {
                    if (field.Permissions.Count > 0)
                    {
                        foreach (var permission in field.Permissions)
                        {
                            if (AppUser.ProfileId == permission.ProfileId && permission.Type == FieldPermissionType.None && !record[field.Name].IsNullOrEmpty())
                            {
                                record[field.Name] = null;
                            }
                        }
                    }
                }

                record = await Model.Helpers.RecordHelper.FormatRecordValues(moduleEntity, record, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset, lookupModules);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.UndefinedTable)
                {
                    return NotFound();
                }

                throw;
            }

            Aspose.Words.Document doc;

            // Open a template document.
            using (var template = new MemoryStream())
            {
                    await templateBlob.DownloadToStreamAsync(template, Microsoft.WindowsAzure.Storage.AccessCondition.GenerateEmptyCondition(), new Microsoft.WindowsAzure.Storage.Blob.BlobRequestOptions(), new Microsoft.WindowsAzure.Storage.OperationContext());

                    doc = new Aspose.Words.Document(template);

            }

            // Add related module records.
            await AddRelatedModuleRecords(relatedModuleRecords, notes, moduleEntity, lookupModules, doc, record, module, id, currentCulture, timezoneOffset);

            doc.MailMerge.UseNonMergeFields = true;
            doc.MailMerge.CleanupOptions = MailMergeCleanupOptions.RemoveUnusedRegions | MailMergeCleanupOptions.RemoveUnusedFields;
            doc.MailMerge.FieldMergingCallback = new FieldMergingCallback(AppUser.TenantGuid, _configuration);

            var mds = new MailMergeDataSource(record, module, moduleEntity, relatedModuleRecords, notes: notes);

            try
            {
                doc.MailMerge.ExecuteWithRegions(mds);
            }
            catch (Exception ex)
            {
                return BadRequest(AppUser.TenantLanguage == "tr" ? "Geçersiz şablon. Lütfen şablon içerisindeki etiketleri kontrol ediniz. Hata Mesajı: " + ex.Message : "Invalid template. Please check tags in templates. Error Message: " + ex.Message);
            }

            var rMessage = new HttpResponseMessage();
            Stream outputStream = new MemoryStream();

            Aspose.Words.SaveFormat sf;
            var localModuleName = AppUser.TenantLanguage.Contains("tr") ? moduleEntity.LabelTrSingular : moduleEntity.LabelEnSingular;
            var fileName = $"{localModuleName}";
            switch (format)
            {
                case "pdf":
                    sf = Aspose.Words.SaveFormat.Pdf;
                    fileName = $"{fileName}.pdf";
                    break;
                case "docx":
                    sf = Aspose.Words.SaveFormat.Docx;
                    fileName = $"{fileName}.docx";
                    break;
                default:
                    sf = Aspose.Words.SaveFormat.Docx;
                    fileName = $"{fileName}.docx";
                    break;
            }

            Aspose.Words.Saving.SaveOptions saveOptions = Aspose.Words.Saving.SaveOptions.CreateSaveOptions(sf);

            doc.Save(outputStream, saveOptions);
            outputStream.Position = 0;
            var mimeType = MimeUtility.GetMimeMapping(fileName);
            if (save)
            {

                await AzureStorage.UploadFile(0, outputStream, "temp", fileName, mimeType, _configuration);
                var blob = await AzureStorage.CommitFile(fileName, Guid.NewGuid().ToString().Replace("-", "") + "." + format, mimeType, "pub", 1, _configuration);

                outputStream.Position = 0;
                var blobUrl = _configuration.GetSection("AppSettings")["BlobUrl"];
                var result = new { filename = fileName, fileurl = $"{blobUrl}{blob.Uri.AbsolutePath}" };

                return Ok(result);
            }
            //rMessage.Content = new StreamContent(outputStream);
            //rMessage.Content.Headers.ContentType = new MediaTypeHeaderValue(mimeType);
            //rMessage.StatusCode = HttpStatusCode.OK;
            //rMessage.Content.Headers.ContentDisposition = new ContentDispositionHeaderValue("attachment")
            //{
            //    FileNameStar = fileName
            //};

            //// TODO: Test this !

            //var response = new ContentResult
            //{
            //    Content = rMessage.Content.ToString(),
            //    ContentType = rMessage.Content.GetType().ToString(),
            //    StatusCode = (int)rMessage.StatusCode
            //};

            ////var response = ResponseMessage(rMessage);

            return File(outputStream, mimeType, fileName, true);
        }

        private async Task<bool> AddRelatedModuleRecords(Dictionary<string, JArray> relatedModuleRecords, List<Note> notes, Module moduleEntity, ICollection<Module> lookupModules, Aspose.Words.Document doc, JObject record, string module, int recordId, string currentCulture, int timezoneOffset)
        {
            // Get second level relations
            var secondLevelRelationSetting = _settingsRepository.Get(SettingType.Template);
            var secondLevels = new List<SecondLevel>();

            if (secondLevelRelationSetting != null && secondLevelRelationSetting.Count > 0)
            {
                var secondLevelRelations = secondLevelRelationSetting.Where(x => x.Key == "second_level_relation_" + module).ToList();

                foreach (var secondLevelRelation in secondLevelRelations)
                {
                    var secondLevelRelationParts = secondLevelRelation.Value.Split('>');
                    var secondLevelModuleRelationId = int.Parse(secondLevelRelationParts[0]);
                    var secondLevelSubModuleRelationId = int.Parse(secondLevelRelationParts[1]);
                    var secondLevelModuleRelation = moduleEntity.Relations.FirstOrDefault(x => !x.Deleted && x.Id == secondLevelModuleRelationId);

                    if (secondLevelModuleRelation != null)
                    {
                        var secondLevelModuleEntity = await _moduleRepository.GetByName(secondLevelModuleRelation.RelatedModule);
                        var secondLevelSubModuleRelation = secondLevelModuleEntity.Relations.FirstOrDefault(x => !x.Deleted && x.Id == secondLevelSubModuleRelationId);
                        Module secondLevelSubModuleEntity = null;

                        if (secondLevelSubModuleRelation != null)
                        {
                            secondLevelSubModuleEntity = await _moduleRepository.GetByName(secondLevelSubModuleRelation.RelatedModule);
                        }
                        else
                        {
                            secondLevelModuleEntity = null;
                        }

                        if (secondLevelModuleEntity != null)
                        {
                            secondLevels.Add(new SecondLevel
                            {
                                RelationId = secondLevelModuleRelationId,
                                Module = secondLevelModuleEntity,
                                SubModule = secondLevelSubModuleEntity,
                                SubRelation = secondLevelSubModuleRelation
                            });
                        }
                    }
                }
            }

            // Get related module records.
            foreach (Relation relation in moduleEntity.Relations.Where(x => !x.Deleted && x.RelationType != RelationType.ManyToMany).ToList())
            {
                if (!doc.Range.Text.Contains("{{#foreach " + relation.RelatedModule + "}}"))
                {
                    continue;
                }

                var fields = await _recordHelper.GetAllFieldsForFindRequest(relation.RelatedModule);

                var findRequest = new FindRequest
                {
                    Fields = fields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = relation.RelationField,
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        }
                    },
                    SortField = "created_at",
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                };

                if (relation.RelatedModule == "activities")
                {
                    findRequest.Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = "related_to",
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        },
                        new Filter
                        {
                            Field = "related_module",
                            Operator = Operator.Is,
                            No = 2,
                            Value = AppUser.TenantLanguage.Contains("tr") ? moduleEntity.LabelTrSingular : moduleEntity.LabelEnSingular
                        }
                    };
                }

                var records = _recordRepository.Find(relation.RelatedModule, findRequest);
                Module relatedModuleEntity;
                ICollection<Module> relatedLookupModules;

                if (moduleEntity.Name == relation.RelatedModule)
                {
                    relatedModuleEntity = moduleEntity;
                    relatedLookupModules = lookupModules;
                }
                else
                {
                    relatedModuleEntity = await _moduleRepository.GetByNameBasic(relation.RelatedModule);
                    relatedLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(relatedModuleEntity, _moduleRepository);
                }

                var recordsFormatted = new JArray();
                var secondLevel = secondLevels.SingleOrDefault(x => x.RelationId == relation.Id);

                foreach (JObject recordItem in records)
                {
                    var recordFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(relatedModuleEntity, recordItem, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset, relatedLookupModules);

                    if (secondLevel != null)
                    {
                        await AddSecondLevelRecords(recordFormatted, secondLevel.Module, secondLevel.SubRelation, (int)recordItem["id"], secondLevel.SubModule, currentCulture, timezoneOffset);
                    }

                    recordsFormatted.Add(recordFormatted);
                }

                relatedModuleRecords.Add(relation.RelatedModule, recordsFormatted);
            }

            // Many to many related module records
            foreach (Relation relation in moduleEntity.Relations.Where(x => !x.Deleted && x.RelationType == RelationType.ManyToMany).ToList())
            {
                if (!doc.Range.Text.Contains("{{#foreach " + relation.RelatedModule + "}}"))
                {
                    continue;
                }

                var fields = await _recordHelper.GetAllFieldsForFindRequest(relation.RelatedModule, false);
                var fieldsManyToMany = new List<string>();

                foreach (var field in fields)
                {
                    fieldsManyToMany.Add(relation.RelatedModule + "_id." + relation.RelatedModule + "." + field);
                }

                var records = _recordRepository.Find(relation.RelatedModule, new FindRequest
                {
                    Fields = fieldsManyToMany,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = moduleEntity.Name + "_id",
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        }
                    },
                    ManyToMany = moduleEntity.Name,
                    SortField = relation.RelatedModule + "_id." + relation.RelatedModule + ".created_at",
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                });

                Module relatedModuleEntity;
                ICollection<Module> relatedLookupModules;

                if (moduleEntity.Name == relation.RelatedModule)
                {
                    relatedModuleEntity = moduleEntity;
                    relatedLookupModules = new List<Module> { moduleEntity };
                }
                else
                {
                    relatedModuleEntity = await _moduleRepository.GetByNameBasic(relation.RelatedModule);
                    relatedLookupModules = new List<Module> { relatedModuleEntity };
                }

                var recordsFormatted = new JArray();
                var secondLevel = secondLevels.SingleOrDefault(x => x.RelationId == relation.Id);

                foreach (JObject recordItem in records)
                {
                    var recordFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(relatedModuleEntity, recordItem, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset, relatedLookupModules);

                    if (secondLevel != null)
                    {
                        await AddSecondLevelRecords(recordFormatted, secondLevel.Module, secondLevel.SubRelation, (int)recordItem[relation.RelatedModule + "_id"], secondLevel.SubModule, currentCulture, timezoneOffset);
                    }

                    recordsFormatted.Add(recordFormatted);
                }

                relatedModuleRecords.Add(relation.RelatedModule, recordsFormatted);
            }

            // Get notes of the record.
            if (doc.Range.Text.Contains("{{#foreach notes}}"))
            {
                var noteList = await _noteRepository.Find(new NoteRequest()
                {
                    Limit = 1000,
                    ModuleId = moduleEntity.Id,
                    Offset = 0,
                    RecordId = recordId
                });
                // NOTES kısmı  için html tagları temizlendi.
                foreach (var item in noteList)
                {
                    item.Text = Regex.Replace(item.Text, "<.*?>", string.Empty).Replace("&nbsp;", " ");
                }
                notes.AddRange(noteList);
            }

            if (module == "quotes" && doc.Range.Text.Contains("{{#foreach quote_products}}"))
            {
                var quoteFields = await _recordHelper.GetAllFieldsForFindRequest("quote_products");

                var products = _recordRepository.Find("quote_products", new FindRequest()
                {
                    Fields = quoteFields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = "quote",
                            Operator = Operator.Equals,
                            No = 0,
                            Value = recordId.ToString()
                        }
                    },
                    SortField = "order",
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                });

                var quoteProductsModuleEntity = await _moduleRepository.GetByNameBasic("quote_products");
                var quoteProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(quoteProductsModuleEntity, _moduleRepository);
                var productsFormatted = new JArray();

                foreach (var product in products)
                {
                    if (!record["currency"].IsNullOrEmpty())
                    {
                        product["currency"] = (string)record["currency"];
                    }

                    if (!product["product.products.currency"].IsNullOrEmpty())
                    {
                        product["currency"] = (string)product["product.products.currency"];
                    }

                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(quoteProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset, quoteProductsLookupModules);
                    if (!productFormatted["separator"].IsNullOrEmpty())
                    {
                        productFormatted["product.products.name"] = productFormatted["separator"] + "-product_separator_separator";
                    }

                    productsFormatted.Add(productFormatted);
                }

                relatedModuleRecords.Add("quote_products", productsFormatted);
            }

            if (module == "sales_orders" && doc.Range.Text.Contains("{{#foreach order_products}}"))
            {
                var orderFields = await _recordHelper.GetAllFieldsForFindRequest("order_products");

                var products = _recordRepository.Find("order_products", new FindRequest()
                {
                    Fields = orderFields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = "sales_order",
                            Operator = Operator.Equals,
                            No = 0,
                            Value = recordId.ToString()
                        }
                    },
                    SortField = "order",
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                });

                var orderProductsModuleEntity = await _moduleRepository.GetByNameBasic("order_products");
                var orderProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(orderProductsModuleEntity, _moduleRepository);
                var productsFormatted = new JArray();

                foreach (var product in products)
                {
                    if (!record["currency"].IsNullOrEmpty())
                    {
                        product["currency"] = (string)record["currency"];
                    }

                    if (!product["product.products.currency"].IsNullOrEmpty())
                    {
                        product["currency"] = (string)product["product.products.currency"];
                    }

                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(orderProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset, orderProductsLookupModules);
                    productsFormatted.Add(productFormatted);
                }

                relatedModuleRecords.Add("order_products", productsFormatted);
            }
            if (module == "purchase_orders" && doc.Range.Text.Contains("{{#foreach purchase_order_products}}"))
            {
                var orderFields = await _recordHelper.GetAllFieldsForFindRequest("purchase_order_products");

                var products = _recordRepository.Find("purchase_order_products", new FindRequest()
                {
                    Fields = orderFields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = "purchase_order",
                            Operator = Operator.Equals,
                            No = 0,
                            Value = recordId.ToString()
                        }
                    },
                    SortField = "order",
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                });

                var orderProductsModuleEntity = await _moduleRepository.GetByNameBasic("purchase_order_products");
                var orderProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(orderProductsModuleEntity, _moduleRepository);
                var productsFormatted = new JArray();

                foreach (var product in products)
                {
                    if (!record["currency"].IsNullOrEmpty())
                    {
                        product["currency"] = (string)record["currency"];
                    }

                    if (!product["product.products.currency"].IsNullOrEmpty())
                    {
                        product["currency"] = (string)product["product.products.currency"];
                    }

                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(orderProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset, orderProductsLookupModules);
                    productsFormatted.Add(productFormatted);
                }

                relatedModuleRecords.Add("purchase_order_products", productsFormatted);
            }
            return true;
        }

        private async Task<bool> AddSecondLevelRecords(JObject record, Module secondLevelModuleEntity, Relation relation, int recordId, Module secondLevelSubModuleEntity, string currentCulture, int timezoneOffset)
        {
            record[secondLevelSubModuleEntity.Name] = "";
            var primaryField = secondLevelSubModuleEntity.Fields.Single(x => x.Primary);
            JArray records;

            if (relation.RelationType != RelationType.ManyToMany)
            {
                var fields = await _recordHelper.GetAllFieldsForFindRequest(relation.RelatedModule);

                var secondLevelFindRequest = new FindRequest
                {
                    Fields = fields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = relation.RelationField,
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        }
                    },
                    SortField = primaryField.Name,
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                };

                records = _recordRepository.Find(secondLevelSubModuleEntity.Name, secondLevelFindRequest);
                var recordsFormatted = new JArray();

                foreach (JObject recordItem in records)
                {
                    var recordItemFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(secondLevelSubModuleEntity, recordItem, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset);
                    record[secondLevelSubModuleEntity.Name] += (string)recordItemFormatted[primaryField.Name] + ControlChar.LineBreak;
                    recordsFormatted.Add(recordItemFormatted);
                }

                record[secondLevelSubModuleEntity.Name + "_records"] = recordsFormatted;
            }
            else
            {
                var fields = await _recordHelper.GetAllFieldsForFindRequest(relation.RelatedModule, false);
                var fieldsManyToMany = new List<string>();

                foreach (var field in fields)
                {
                    fieldsManyToMany.Add(relation.RelatedModule + "_id." + relation.RelatedModule + "." + field);
                }

                var secondLevelFindRequestManyToMany = new FindRequest
                {
                    Fields = fieldsManyToMany,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = secondLevelModuleEntity.Name + "_id",
                            Operator = Operator.Equals,
                            No = 1,
                            Value = recordId.ToString()
                        }
                    },
                    ManyToMany = secondLevelModuleEntity.Name,
                    SortField = relation.RelatedModule + "_id." + relation.RelatedModule + "." + primaryField.Name,
                    SortDirection = SortDirection.Asc,
                    Limit = 1000,
                    Offset = 0
                };

                records = _recordRepository.Find(relation.RelatedModule, secondLevelFindRequestManyToMany);
                var recordsFormatted = new JArray();

                foreach (JObject recordItem in records)
                {
                    var recordItemFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(secondLevelSubModuleEntity, recordItem, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantLanguage, currentCulture, timezoneOffset);
                    record[secondLevelSubModuleEntity.Name] += (string)recordItemFormatted[relation.RelatedModule + "_id." + relation.RelatedModule + "." + primaryField.Name] + ControlChar.LineBreak;
                    recordsFormatted.Add(recordItemFormatted);
                }

                record[secondLevelSubModuleEntity.Name + "_records"] = recordsFormatted;
            }

            return true;
        }

        [Route("UploadAvatar"), HttpPost]
        public async Task<IActionResult> UploadAvatar()
        {

            HttpMultipartParser parser = new HttpMultipartParser(Request.Body, "file");

            if (parser.Success)
            {
                //if succesfully parsed, then continue to thread.
                if (parser.FileContents.Length <= 0)
                {
                    //if file is invalid, then stop thread and return bad request status code.
                    return BadRequest();
                }

                //initialize chunk parameters for the upload.
                int chunk = 0;
                int chunks = 1;

                var uniqueName = string.Empty;

                if (parser.Parameters.Count > 1)
                {
                    //this is a chunked upload process, calculate how many chunks we have.
                    chunk = int.Parse(parser.Parameters["chunk"]);
                    chunks = int.Parse(parser.Parameters["chunks"]);

                    //get the file name from parser
                    if (parser.Parameters.ContainsKey("name"))
                    {
                        uniqueName = parser.Parameters["name"];
                    }
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid() + ext;
                }

                //upload file to the temporary AzureStorage.
                AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType, _configuration).Wait();

                if (chunk == chunks - 1)
                {
                    //if this is last chunk, then move the file to the permanent storage by commiting it.
                    //as a standart all avatar files renamed to UserID_UniqueFileName format.
                    var user_image = string.Format("{0}_{1}", AppUser.Id, uniqueName);
                    await AzureStorage.CommitFile(uniqueName, user_image, parser.ContentType, "user-images", chunks, _configuration);
                    return Ok(user_image);
                }

                //return content type.
                return Ok(parser.ContentType);
            }
            //this is not a valid request so return fail.
            return Ok("Fail");
        }


        [Route("upload_logo")]
        [ProducesResponseType(typeof(string), 200)]
        //[ResponseType(typeof(string))]
        [HttpPost]
        public async Task<IActionResult> UploadLogo()
        {
            HttpMultipartParser parser = new HttpMultipartParser(Request.Body, "file");

            if (parser.Success)
            {
                //if succesfully parsed, then continue to thread.
                if (parser.FileContents.Length <= 0)
                {
                    //if file is invalid, then stop thread and return bad request status code.
                    return BadRequest();
                }

                //initialize chunk parameters for the upload.
                int chunk = 0;
                int chunks = 1;

                var uniqueName = string.Empty;

                if (parser.Parameters.Count > 1)
                {
                    //this is a chunked upload process, calculate how many chunks we have.
                    chunk = int.Parse(parser.Parameters["chunk"]);
                    chunks = int.Parse(parser.Parameters["chunks"]);

                    //get the file name from parser
                    if (parser.Parameters.ContainsKey("name"))
                    {
                        uniqueName = parser.Parameters["name"];
                    }
                }

                if (string.IsNullOrEmpty(uniqueName))
                {
                    var ext = Path.GetExtension(parser.Filename);
                    uniqueName = Guid.NewGuid() + ext;
                }

                //upload file to the temporary AzureStorage.
                AzureStorage.UploadFile(chunk, new MemoryStream(parser.FileContents), "temp", uniqueName, parser.ContentType, _configuration).Wait();

                if (chunk == chunks - 1)
                {
                    //if this is last chunk, then move the file to the permanent storage by commiting it.
                    //as a standart all avatar files renamed to UserID_UniqueFileName format.
                    var logo = string.Format("{0}_{1}", AppUser.TenantGuid, uniqueName);
                    await AzureStorage.CommitFile(uniqueName, logo, parser.ContentType, "app-logo", chunks, _configuration);
                    return Ok(logo);
                }

                //return content type.
                return Ok(parser.ContentType);
            }
            //this is not a valid request so return fail.
            return Ok("Fail");
        }

        [Route("download")]
        public async Task<EmptyResult> Download([FromQuery(Name = "fileId")] int FileId)
        {
            var doc = await _documentRepository.GetById(FileId);
            if (doc != null)
            {
                //if there is a document with this id, try to get it from blob AzureStorage.
                var blob = AzureStorage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), doc.UniqueName, _configuration);
                try
                {
                    //try to get the attributes of blob.
                    await blob.FetchAttributesAsync();
                }
                catch (Exception ex)
                {
                    //if there is an exception, it means there is no such file.
                    throw ex;
                }


                //return Redirect(blob.Uri.AbsoluteUri + blob.GetSharedAccessSignature(new Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPolicy()
                //{
                //    Permissions = Microsoft.WindowsAzure.Storage.Blob.SharedAccessBlobPermissions.Read,
                //    SharedAccessExpiryTime = DateTime.UtcNow.AddSeconds(5),
                //    SharedAccessStartTime = DateTime.UtcNow.AddSeconds(-5)
                //}));

                Response.Headers.Add("Content-Disposition", "attachment; filename=" + doc.Name); // force download
                await blob.DownloadToStreamAsync(Response.Body);

                return new EmptyResult();
            }
            else
            {
                //there is no such file, return
                throw new Exception("Document does not exist in the storage!");
            }


        }

        [Route("download_template"), HttpGet]
        public async Task<IActionResult> DownloadTemplate([FromQuery(Name = "template_id")]int templateId)
        {
            //get the document record from database
            var template = await _templateRepository.GetById(templateId);
            string publicName = "";

            if (template != null)
            {
                //if there is a document with this id, try to get it from blob AzureStorage.
                var blob = AzureStorage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), $"templates/{template.Content}", _configuration);
                try
                {
                    //try to get the attributes of blob.
                    await blob.FetchAttributesAsync();
                }
                catch (Exception)
                {
                    //if there is an exception, it means there is no such file.
                    return NotFound();
                }

                //Bandwidth is enough, send the AzureStorage.
                publicName = template.Name;

                string[] splittedFileName = template.Content.Split('.');
                string extension = splittedFileName.Length > 1 ? splittedFileName[1] : "xlsx";

                Response.Headers.Add("Content-Disposition", "attachment; filename=" + $"{template.Name}.{extension}"); // force download
                await blob.DownloadToStreamAsync(Response.Body);

                return new EmptyResult();
            }
            else
            {
                //there is no such file, return
                return NotFound();
            }
        }

        [Route("export_excel")]
        public async Task<ActionResult> ExportExcel([FromQuery(Name = "module")]string module)
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                throw new HttpRequestException("Module field is required");
            }

            var moduleEntity = await _moduleRepository.GetByName(module);
            var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
            var nameModule = AppUser.Culture.Contains("tr") ? moduleEntity.LabelTrPlural : moduleEntity.LabelEnPlural;
            //byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(nameModule);
            //var moduleName = System.Text.Encoding.ASCII.GetString(bytes);
            Workbook workbook = new Workbook(FileFormatType.Xlsx);
            Worksheet worksheetData = workbook.Worksheets[0];
            worksheetData.Name = "Data";
            DataTable dt = new DataTable("Excel");
            Worksheet worksheet2 = workbook.Worksheets.Add("Report Formula");
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository);

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

            var records = _recordRepository.Find(moduleEntity.Name, findRequest);

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

            Stream memory = new MemoryStream();

            var fileName = nameModule + ".xlsx";

            workbook.Save(memory, Aspose.Cells.SaveFormat.Xlsx);
            memory.Position = 0;

            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }



        [Route("export_excel_view")]
        public async Task<ActionResult> ExportExcelView([FromQuery(Name = "module")]string module, string locale = "", bool? normalize = false, int? timezoneOffset = 180)
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                throw new HttpRequestException("Module field is required");
            }

            var moduleEntity = await _moduleRepository.GetByName(module);
            var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
            var nameModule = AppUser.Culture.Contains("tr") ? moduleEntity.LabelTrPlural : moduleEntity.LabelEnPlural;
            //byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(nameModule);
            //var moduleName = System.Text.Encoding.ASCII.GetString(bytes);
            Workbook workbook = new Workbook(FileFormatType.Xlsx);
            Worksheet worksheetData = workbook.Worksheets[0];
            worksheetData.Name = "Data";
            DataTable dt = new DataTable("Excel");
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository);

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

            var records = _recordRepository.Find(moduleEntity.Name, findRequest);

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

            Stream memory = new MemoryStream();

            var fileName = nameModule + ".xlsx";

            workbook.Save(memory, Aspose.Cells.SaveFormat.Xlsx);
            memory.Position = 0;

            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [Route("export_excel_no_data")]
        public async Task<FileStreamResult> ExportExcelNoData(string module, int templateId, string templateName, string locale = "", bool? normalize = false, int? timezoneOffset = 180)
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                throw new HttpRequestException("Module field is required");
            }

            var moduleEntity = await _moduleRepository.GetByName(module);
            var Module = await _moduleRepository.GetByName(module);
            var template = await _templateRepository.GetById(templateId);
            var blob = AzureStorage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), $"templates/{template.Content}", _configuration);
            var fields = Module.Fields.OrderBy(x => x.Id).ToList();
            //var tempsName = templateName;
            //byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(tempsName);
            //var tempName = System.Text.Encoding.ASCII.GetString(bytes);
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository);

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

            var records = _recordRepository.Find(moduleEntity.Name, findRequest);

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
                            var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
                            var primaryField = lookupModule.Fields.Single(x => x.Primary);
                            dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];
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

                Stream memory = new MemoryStream();

                var fileName = templateName + ".xlsx";

                workbook.Save(memory, Aspose.Cells.SaveFormat.Xlsx);
                memory.Position = 0;

                return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [Route("export_excel_data")]
        public async Task<FileStreamResult> ExportExcelData(string module, string templateName, int templateId, string locale = "", bool? normalize = false, int? timezoneOffset = 180)
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                throw new HttpRequestException("Module field is required");
            }

            var moduleEntity = await _moduleRepository.GetByName(module);
            var template = await _templateRepository.GetById(templateId);
            var blob = AzureStorage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), $"templates/{template.Content}", _configuration);
            var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
            //var tempsName = templateName;
            //byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(tempsName);
            //var tempName = System.Text.Encoding.ASCII.GetString(bytes);
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository);

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

            var records = _recordRepository.Find(moduleEntity.Name, findRequest);

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
                            var lookupModule = lookupModules.Single(x => x.Name == field.LookupType);
                            var primaryField = lookupModule.Fields.Single(x => x.Primary);
                            dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];
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

                Stream memory = new MemoryStream();

                var fileName = templateName + ".xlsx";

                workbook.Save(memory, Aspose.Cells.SaveFormat.Xlsx);
                memory.Position = 0;

                return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

            }
        }
    }
}

