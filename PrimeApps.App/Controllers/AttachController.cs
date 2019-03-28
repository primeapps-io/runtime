using Aspose.Cells;
using Aspose.Words;
using Aspose.Words.MailMerging;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MimeMapping;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Npgsql;
using PrimeApps.App.Extensions;
using PrimeApps.App.Helpers;
using PrimeApps.App.Storage;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
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
using PrimeApps.App.Models;
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
        private IServiceScopeFactory _serviceScopeFactory;
        private IViewRepository _viewRepository;
        private IUnifiedStorage _storage;

        private IRecordHelper _recordHelper;
        public AttachController(ITenantRepository tenantRepository, IDocumentRepository documentRepository, IModuleRepository moduleRepository, IRecordRepository recordRepository, ITemplateRepository templateRepository, IPicklistRepository picklistRepository, ISettingRepository settingsRepository, IRecordHelper recordHelper, INoteRepository noteRepository, IConfiguration configuration, IHostingEnvironment hostingEnvironment, IUnifiedStorage storage, IServiceScopeFactory serviceScopeFactory, IViewRepository viewRepository)
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
            _serviceScopeFactory = serviceScopeFactory;
            _viewRepository = viewRepository;
            _storage = storage;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_tenantRepository);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_templateRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_documentRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_picklistRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_settingsRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_noteRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_viewRepository, PreviewMode, TenantId, AppId);
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
            if (!await _storage.ObjectExists(UnifiedStorage.GetPath("template", AppUser.TenantId), templateEntity.Content))
            {
                return NotFound();
            }

            //if there is a template with this id, try to get it from blob AzureStorage.
            // var templateBlob = AzureStorage.GetBlob(string.Format("inst-{0}", AppUser.TenantGuid), $"templates/{templateEntity.Content}", _configuration);

            // try
            // {
            //     //try to get the attributes of blob.
            //     await templateBlob.FetchAttributesAsync();
            // }
            // catch (Exception)
            // {
            //     //if there is an exception, it means there is no such file.
            //     return NotFound();
            // }

            if (module == "users")
            {
                moduleEntity = Model.Helpers.ModuleHelper.GetFakeUserModule();
            }

            if (module == "profiles")
                moduleEntity = Model.Helpers.ModuleHelper.GetFakeProfileModule();

            if (module == "roles")
                moduleEntity = Model.Helpers.ModuleHelper.GetFakeRoleModule(AppUser.TenantLanguage);


            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository, tenantLanguage: AppUser.TenantLanguage);

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

                record = await Model.Helpers.RecordHelper.FormatRecordValues(moduleEntity, record, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset, lookupModules);
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
            using (var template = await _storage.Client.GetObjectStreamAsync(UnifiedStorage.GetPath("template", AppUser.TenantId), templateEntity.Content, null))
            {
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
            var primaryField = moduleEntity.Fields.Single(x => x.Primary);
            var fileName = $"{templateEntity.Name} - {record[primaryField.Name]}";
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
            string publicFileName = Guid.NewGuid().ToString().Replace("-", "") + "." + format;
            string publicPath = UnifiedStorage.GetPath("public", AppUser.TenantId);
            if (save)
            {

                await _storage.Upload(publicPath, publicFileName, outputStream);

                outputStream.Position = 0;
                var blobUrl = _configuration.GetValue("AppSettings:StorageUrl", string.Empty);
				var result = new { filename = fileName, fileurl = _storage.GetShareLink(publicPath, publicFileName, DateTime.UtcNow.AddYears(100)) };

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
            var templateSettings = _settingsRepository.Get(SettingType.Template);
            var secondLevels = new List<SecondLevel>();
            var relationSorts = new Dictionary<string, string>();

            if (templateSettings != null && templateSettings.Count > 0)
            {
                var secondLevelRelations = templateSettings.Where(x => x.Key == "second_level_relation_" + module).ToList();
                var relationsSortSettings = templateSettings.Where(x => x.Key == "relation_sorts_" + module).ToList();

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

                foreach (var relationsSort in relationsSortSettings)
                {
                    var relationsSortParts = relationsSort.Value.Split('|');

                    foreach (var relationsSortPart in relationsSortParts)
                    {
                        var sortParts = relationsSortPart.Split(';');
                        relationSorts.Add(sortParts[0], sortParts[1]);
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

                var sortField = "created_at";
                var sortDirection = SortDirection.Asc;

                if (relationSorts.Count > 0)
                {
                    if (relationSorts.ContainsKey(relation.RelatedModule))
                    {
                        var relationSort = relationSorts[relation.RelatedModule].Split(',');

                        sortField = relationSort[0];
                        sortDirection = (SortDirection)Enum.Parse(typeof(SortDirection), relationSort[1]);
                    }
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
                    SortField = sortField,
                    SortDirection = sortDirection,
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
                    relatedLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(relatedModuleEntity, _moduleRepository, tenantLanguage: AppUser.TenantLanguage);
                }

                var recordsFormatted = new JArray();
                var secondLevel = secondLevels.SingleOrDefault(x => x.RelationId == relation.Id);

                foreach (JObject recordItem in records)
                {
                    var recordFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(relatedModuleEntity, recordItem, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset, relatedLookupModules);

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
                    var recordFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(relatedModuleEntity, recordItem, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset, relatedLookupModules);

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
                var quoteProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(quoteProductsModuleEntity, _moduleRepository, tenantLanguage: AppUser.TenantLanguage);
                var productsFormatted = new JArray();
                int orderCount = 1;

                foreach (var product in products)
                {
                    if (product["currency"].IsNullOrEmpty())
                    {
                        if (!product["product.products.currency"].IsNullOrEmpty())
                            product["currency"] = (string)product["product.products.currency"];

                        if (!record["currency"].IsNullOrEmpty())
                            product["currency"] = (string)record["currency"];
                    }
                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(quoteProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset, quoteProductsLookupModules);

                    if (!productFormatted["separator"].IsNullOrEmpty())
                    {
                        productFormatted["order"] = null;
                        orderCount = 1;
                        productFormatted["product.products.name"] = productFormatted["separator"] + "-product_separator_separator";
                    }
                    else
                    {
                        productFormatted["order"] = orderCount.ToString();
                        orderCount++;
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
                var orderProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(orderProductsModuleEntity, _moduleRepository, tenantLanguage: AppUser.TenantLanguage);
                var productsFormatted = new JArray();

                foreach (var product in products)
                {
                    if (product["currency"].IsNullOrEmpty())
                    {
                        if (!product["product.products.currency"].IsNullOrEmpty())
                            product["currency"] = (string)product["product.products.currency"];

                        if (!record["currency"].IsNullOrEmpty())
                            product["currency"] = (string)record["currency"];
                    }

                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(orderProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset, orderProductsLookupModules);

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
                var orderProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(orderProductsModuleEntity, _moduleRepository, tenantLanguage: AppUser.TenantLanguage);
                var productsFormatted = new JArray();

                foreach (var product in products)
                {
                    if (product["currency"].IsNullOrEmpty())
                    {
                        if (!product["product.products.currency"].IsNullOrEmpty())
                            product["currency"] = (string)product["product.products.currency"];

                        if (!record["currency"].IsNullOrEmpty())
                            product["currency"] = (string)record["currency"];
                    }

                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(orderProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset, orderProductsLookupModules);
                    productsFormatted.Add(productFormatted);
                }

                relatedModuleRecords.Add("purchase_order_products", productsFormatted);
            }

            if (module == "sales_invoices" && doc.Range.Text.Contains("{{#foreach purchase_order_products}}"))
            {
                var orderFields = await _recordHelper.GetAllFieldsForFindRequest("sales_invoices_products");

                var products = _recordRepository.Find("sales_invoices_products", new FindRequest()
                {
                    Fields = orderFields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = "sales_invoice",
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

                var orderProductsModuleEntity = await _moduleRepository.GetByNameBasic("sales_invoices_products");
                var orderProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(orderProductsModuleEntity, _moduleRepository);
                var productsFormatted = new JArray();

                foreach (var product in products)
                {
                    if (product["currency"].IsNullOrEmpty())
                    {
                        if (!product["product.products.currency"].IsNullOrEmpty())
                            product["currency"] = (string)product["product.products.currency"];

                        if (!record["currency"].IsNullOrEmpty())
                            product["currency"] = (string)record["currency"];
                    }
                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(orderProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset, orderProductsLookupModules);

                    productsFormatted.Add(productFormatted);
                }

                relatedModuleRecords.Add("sales_invoices_products", productsFormatted);
            }

            if (module == "purchase_invoices" && doc.Range.Text.Contains("{{#foreach purchase_invoices_products}}"))
            {
                var orderFields = await _recordHelper.GetAllFieldsForFindRequest("sales_invoices_products");

                var products = _recordRepository.Find("purchase_invoices_products", new FindRequest()
                {
                    Fields = orderFields,
                    Filters = new List<Filter>
                    {
                        new Filter
                        {
                            Field = "purchase_invoice",
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

                var orderProductsModuleEntity = await _moduleRepository.GetByNameBasic("purchase_invoices_products");
                var orderProductsLookupModules = await Model.Helpers.RecordHelper.GetLookupModules(orderProductsModuleEntity, _moduleRepository);
                var productsFormatted = new JArray();

                foreach (var product in products)
                {
                    if (product["currency"].IsNullOrEmpty())
                    {
                        if (!product["product.products.currency"].IsNullOrEmpty())
                            product["currency"] = (string)product["product.products.currency"];

                        if (!record["currency"].IsNullOrEmpty())
                            product["currency"] = (string)record["currency"];
                    }

                    var productFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(orderProductsModuleEntity, (JObject)product, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset, orderProductsLookupModules);
                    productsFormatted.Add(productFormatted);
                }

                relatedModuleRecords.Add("purchase_invoices_products", productsFormatted);
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
                    var recordItemFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(secondLevelSubModuleEntity, recordItem, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset);
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
                    var recordItemFormatted = await Model.Helpers.RecordHelper.FormatRecordValues(secondLevelSubModuleEntity, recordItem, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset);
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
        public async Task<ActionResult> ExportExcel([FromQuery(Name = "module")]string module, string locale = "", int? timezoneOffset = 180)
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
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository, tenantLanguage: AppUser.TenantLanguage);
            var currentCulture = locale == "en" ? "en-US" : "tr-TR";
            var formatDate = currentCulture == "tr-TR" ? "dd.MM.yyyy" : "M/d/yyyy";
            var formatDateTime = currentCulture == "tr-TR" ? "dd.MM.yyyy HH:mm" : "M/d/yyyy h:mm a";
            var formatTime = currentCulture == "tr-TR" ? "HH:mm" : "h:mm a";
            var format = "";

            var findRequest = new FindRequest();
            findRequest.Fields = new List<string>();
            findRequest.Limit = 9999;

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                if (field.DataType != Model.Enums.DataType.Lookup)
                {
                    findRequest.Fields.Add(field.Name);
                }
                else
                {
                    var lookupModule = lookupModules.FirstOrDefault(x => x.Name == field.LookupType);
                    var primaryField = new Field();

                    if (lookupModule != null)
                        primaryField = lookupModule.Fields.Single(x => x.Primary);
                    else
                        continue;

                    findRequest.Fields.Add(field.Name + "." + field.LookupType + "." + primaryField.Name);
                }
            }

            var records = _recordRepository.Find(moduleEntity.Name, findRequest);

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];
                if (!dt.Columns.Contains(field.LabelTr))
                {
                    switch (field.DataType)
                    {
                        case DataType.Number:
                        case DataType.NumberAuto:
                        case DataType.NumberDecimal:
                        case DataType.Currency:
                            format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                            dt.Columns.Add(format).DataType = typeof(decimal);
                            break;
                        case DataType.Lookup:
                            format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                            dt.Columns.Add(format);
                            break;
                        default:
                            format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                            dt.Columns.Add(format).DataType = typeof(string);
                            break;
                    }
                }
                else
                    switch (field.DataType)
                    {
                        case DataType.Number:
                        case DataType.NumberAuto:
                        case DataType.NumberDecimal:
                        case DataType.Currency:
                            format = locale.Contains("tr") ? field.LabelTr + " (" + field.Name + ")" : field.LabelEn + " (" + field.Name + ")";
                            dt.Columns.Add(format).DataType = typeof(decimal);
                            break;
                        case DataType.Lookup:
                            format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                            dt.Columns.Add(format);
                            break;
                        default:
                            format = locale.Contains("tr") ? field.LabelTr + " (" + field.Name + ")" : field.LabelEn + " (" + field.Name + ")";
                            dt.Columns.Add(format).DataType = typeof(string);
                            break;
                    }
            }
            for (int j = 0; j < records.Count; j++)
            {
                var record = records[j];
                var dr = dt.NewRow();

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];

                    //Lookuplarda field name boş geliyor.
                    if ((field.DataType.ToString() != "Lookup") && (record[field.Name] == null || record[field.Name].IsNullOrEmpty()))
                    {
                        continue;
                    }

                    switch (field.DataType)
                    {
                        case DataType.Date:
                            record[field.Name] = ((DateTime)record[field.Name]).AddMinutes((int)timezoneOffset).ToString(formatDate);
                            break;
                        case DataType.DateTime:
                            record[field.Name] = ((DateTime)record[field.Name]).AddMinutes((int)timezoneOffset).ToString(formatDateTime);
                            break;
                        case DataType.Time:
                            record[field.Name] = ((DateTime)record[field.Name]).AddMinutes((int)timezoneOffset).ToString(formatTime);
                            break;
                    }


                    if (field.DataType != Model.Enums.DataType.Lookup)
                    {
                        switch (field.DataType)
                        {
                            case DataType.Number:
                            case DataType.NumberAuto:
                            case DataType.NumberDecimal:
                            case DataType.Currency:
                                dr[i] = (decimal)record[field.Name];
                                break;
                            case DataType.Tag:
                            case DataType.Multiselect:
                                var multi = record[field.Name].ToObject<List<string>>();
                                dr[i] = string.Join("|", multi);
                                break;

                            default:
                                dr[i] = record[field.Name];
                                break;
                        }
                    }
                    else
                    {
                        var lookupModule = lookupModules.FirstOrDefault(x => x.Name == field.LookupType);
                        var primaryField = new Field();

                        if (lookupModule != null)
                            primaryField = lookupModule.Fields.Single(x => x.Primary);
                        else
                            continue;

                        dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];
                    }
                }
                dt.Rows.Add(dr);
            }

            worksheetData.Cells.ImportDataTable(dt, true, "A1");

            worksheetData.AutoFitColumns();
            worksheetData.AutoFitRows();

            Stream memory = new MemoryStream();

            var fileName = nameModule + ".xlsx";

            workbook.Save(memory, Aspose.Cells.SaveFormat.Xlsx);
            memory.Position = 0;

            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }



        [Route("export_excel_view")]
        public async Task<ActionResult> ExportExcelView([FromQuery(Name = "module")]string module, int viewId, int profileId, string locale = "", bool? normalize = false, int? timezoneOffset = 180, string listFindRequestJson = "", bool isViewFields = false)
        {
            if (string.IsNullOrWhiteSpace(module))
            {
                throw new HttpRequestException("Module field is required");
            }

            var moduleEntity = await _moduleRepository.GetByName(module);
            var moduleFields = moduleEntity.Fields.Where(x => !x.Deleted).OrderBy(x => x.Id).ToList();
            var nameModule = AppUser.Culture.Contains("tr") ? moduleEntity.LabelTrPlural : moduleEntity.LabelEnPlural;
            var serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();
            //byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(nameModule);
            //var moduleName = System.Text.Encoding.ASCII.GetString(bytes);
            Workbook workbook = new Workbook(FileFormatType.Xlsx);
            Worksheet worksheetData = workbook.Worksheets[0];
            worksheetData.Name = "Data";
            DataTable dt = new DataTable("Excel");
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository, tenantLanguage: AppUser.TenantLanguage);
            var currentCulture = locale == "en" ? "en-US" : "tr-TR";
            var formatDate = currentCulture == "tr-TR" ? "dd.MM.yyyy" : "M/d/yyyy";
            var formatDateTime = currentCulture == "tr-TR" ? "dd.MM.yyyy HH:mm" : "M/d/yyyy h:mm a";
            var formatTime = currentCulture == "tr-TR" ? "HH:mm" : "h:mm a";
            var label = "";
            var fields = new List<Field>();

            foreach (var field in moduleFields)
            {
                var fieldPermission = HasFieldPermission(profileId, field, moduleEntity);

                if (fieldPermission)
                    fields.Add(field);
            }

            var view = await _viewRepository.GetById(viewId);

            if (view == null)
                return null;

            /**
             * listFindRequestJson View'den gelen data
             * listFindRequest View'den gelen datayı Json formatına çeviriyoruz
             */

            FindRequest listFindRequest = null;

            if (!string.IsNullOrWhiteSpace(listFindRequestJson))
            {
                serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();
                listFindRequest = JsonConvert.DeserializeObject<FindRequest>(listFindRequestJson, serializerSettings);
            }

            var findRequest = new FindRequest();
            var newSortField = "id";
            var newSortDirection = SortDirection.Desc;

            if (listFindRequest.SortField != null && listFindRequest.SortDirection != 0)
            {
                newSortField = listFindRequest.SortField;
                newSortDirection = listFindRequest.SortDirection;
            }

            findRequest.Fields = new List<string>();
            findRequest.SortField = newSortField;
            findRequest.SortDirection = newSortDirection;
            findRequest.Limit = 9999;

            if (listFindRequest.Filters != null && listFindRequest.Filters.Count > 0)
            {
                findRequest.Filters = new List<Filter>();

                foreach (var viewFilter in listFindRequest.Filters)
                {
                    findRequest.Filters.Add(new Filter
                    {
                        Field = viewFilter.Field,
                        Operator = viewFilter.Operator,
                        Value = viewFilter.Value,
                        No = viewFilter.No
                    });
                }
            }
            else if (view.Filters != null && view.Filters.Count > 0)
            {
                findRequest.Filters = new List<Filter>();

                foreach (var viewFilter in view.Filters)
                {
                    viewFilter.Value = viewFilter.Value.Replace("[me]", AppUser.TenantId.ToString());
                    viewFilter.Value = viewFilter.Value.Replace("[me.email]", AppUser.Email);

                    findRequest.Filters.Add(new Filter
                    {
                        Field = viewFilter.Field,
                        Operator = viewFilter.Operator,
                        Value = viewFilter.Value,
                        No = viewFilter.No
                    });
                }
            }

            if (!string.IsNullOrEmpty(view.FilterLogic))
                findRequest.FilterLogic = view.FilterLogic;
            /**
             * isViewFields, Modüldeki tüm alanları aktar check boxtan beslenmektedir.
             * Modüldeki tüm alanları aktar Check değil ise -> isViewFields = true
             * isViewFields = true durumunda View'de görüntülenen alanları export etmekteyiz.
             */
            if (isViewFields)
            {
                var viewFields = new List<Field>();
                var field = new Field();
                var viewFieldsList = view.Fields.Where(x => !x.Deleted);

                foreach (var viewField in viewFieldsList)
                {
                    if (viewField.Field.Contains(".primary"))
                        continue;

                    /**
                     * viewField.Field.Contains(".")  ViewFields'lerdeki fieldlerin, fields nameler arasında yer almadığından dolayı split edilmiştir.
                     * örn: ViewFields = calisan.calisan_ad.primary fields.Name = calisan
                     */
                    if (!viewField.Field.Contains("."))
                    {
                        field = fields.FirstOrDefault(x => x.Name == viewField.Field);

                        if (field == null)
                            continue;

                        var fieldJson = JsonConvert.SerializeObject(field, serializerSettings);
                        var fieldClone = JsonConvert.DeserializeObject<Field>(fieldJson, serializerSettings);

                        fieldClone.StyleInput = viewField.Field;//Mecburen eklendi. Fatih Sever ekledi :) Asagidaki döngüde ihtiyaç olduğu için bu sekilde eklendi.

                        viewFields.Add(fieldClone);
                    }
                    else
                    {
                        var viewFieldParts = viewField.Field.Split('.');
                        var viewFieldModule = lookupModules.FirstOrDefault(x => x.Name == viewFieldParts[1] && !x.Deleted);

                        if (viewFieldModule == null)
                            continue;

                        var viewFieldLookup = viewFieldModule.Fields.FirstOrDefault(x => x.Name == viewFieldParts[2] && !x.Deleted);

                        if (viewFieldLookup == null)
                            continue;

                        var viewFieldLookupJson = JsonConvert.SerializeObject(viewFieldLookup, serializerSettings);
                        var viewFieldLookupClone = JsonConvert.DeserializeObject<Field>(viewFieldLookupJson, serializerSettings);

                        viewFieldLookupClone.StyleInput = viewField.Field;//Mecburen eklendi. Fatih Sever ekledi :) Asagidaki döngüde ihtiyaç olduğu için bu sekilde eklendi.

                        viewFields.Add(viewFieldLookupClone);
                    }
                }

                fields = viewFields;
            }

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                if (field.DataType != Model.Enums.DataType.Lookup)
                {
                    findRequest.Fields.Add(field.Name);
                }
                else
                {
                    var lookupModule = lookupModules.FirstOrDefault(x => x.Name == field.LookupType);
                    var primaryField = new Field();

                    if (lookupModule != null)
                        primaryField = lookupModule.Fields.Single(x => x.Primary);
                    else
                        continue;

                    findRequest.Fields.Add(field.Name + "." + field.LookupType + "." + primaryField.Name);
                }
            }

            var records = _recordRepository.Find(moduleEntity.Name, findRequest);


            string labelLocale = "";

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                label = locale.Contains("tr") ? field.LabelTr : field.LabelEn;

                if (field.Module != null && module != field.Module.Name)
                {
                    if (locale.Contains("tr"))
                        labelLocale = label + " " + "(" + field.Module.LabelTrSingular + ")";
                    else
                        labelLocale = label + " " + "(" + field.Module.LabelEnSingular + ")";
                }
                else
                {
                    if (field.StyleInput != null && field.StyleInput.Contains('.'))
                    {
                        var fieldStyleInput = field.StyleInput.Split('.');
                        labelLocale = label + " " + "(" + fieldStyleInput[0] + ")";
                    }
                    else
                    {
                        labelLocale = label;
                    }

                }

                if (!dt.Columns.Contains(labelLocale))
                {
                    switch (field.DataType)
                    {
                        case DataType.Number:
                        case DataType.NumberAuto:
                        case DataType.NumberDecimal:
                        case DataType.Currency:
                            dt.Columns.Add(labelLocale).DataType = typeof(decimal);
                            break;
                        case DataType.Lookup:
                            dt.Columns.Add(labelLocale);
                            break;
                        default:
                            dt.Columns.Add(labelLocale).DataType = typeof(string);
                            break;
                    }
                }
                else
                {
                    if (field.StyleInput != null)
                        labelLocale = locale.Contains("tr") ? field.LabelTr + " (" + field.StyleInput + ")" : field.LabelEn + " (" + field.StyleInput + ")";
                    else
                        labelLocale = locale.Contains("tr") ? field.LabelTr + " (" + field.Name + ")" : field.LabelEn + " (" + field.Name + ")";

                    switch (field.DataType)
                    {
                        case DataType.Number:
                        case DataType.NumberAuto:
                        case DataType.NumberDecimal:
                        case DataType.Currency:
                            dt.Columns.Add(labelLocale).DataType = typeof(decimal);
                            break;
                        case DataType.Lookup:
                            dt.Columns.Add(labelLocale);
                            break;
                        default:
                            dt.Columns.Add(labelLocale).DataType = typeof(string);
                            break;
                    }
                }
            }
            for (int j = 0; j < records.Count; j++)
            {
                var record = records[j];
                var dr = dt.NewRow();

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];

                    //Lookuplarda field name boş geliyor.
                    if (field.DataType != DataType.Lookup && record[!isViewFields ? field.Name : field.StyleInput].IsNullOrEmpty())
                        continue;

                    switch (field.DataType)
                    {
                        case DataType.Date:
                            record[!isViewFields ? field.Name : field.StyleInput] = ((DateTime)record[!isViewFields ? field.Name : field.StyleInput]).AddMinutes((int)timezoneOffset).ToString(formatDate);
                            break;
                        case DataType.DateTime:
                            record[!isViewFields ? field.Name : field.StyleInput] = ((DateTime)record[!isViewFields ? field.Name : field.StyleInput]).AddMinutes((int)timezoneOffset).ToString(formatDateTime);
                            break;
                        case DataType.Time:
                            record[!isViewFields ? field.Name : field.StyleInput] = ((DateTime)record[!isViewFields ? field.Name : field.StyleInput]).AddMinutes((int)timezoneOffset).ToString(formatTime);
                            break;
                    }

                    if (field.DataType != DataType.Lookup)
                    {

                        switch (field.DataType)
                        {
                            case DataType.Number:
                            case DataType.NumberAuto:
                                dr[i] = (int)record[!isViewFields ? field.Name : field.StyleInput];
                                break;
                            case DataType.NumberDecimal:
                            case DataType.Currency:
                                dr[i] = (decimal)record[!isViewFields ? field.Name : field.StyleInput];
                                break;
                            case DataType.Tag:
                            case DataType.Multiselect:
                                var multi = record[!isViewFields ? field.Name : field.StyleInput].ToObject<List<string>>();
                                dr[i] = string.Join("|", multi);
                                break;
                            default:
                                dr[i] = record[!isViewFields ? field.Name : field.StyleInput];
                                break;
                        }
                    }
                    else
                    {
                        var lookupModule = lookupModules.FirstOrDefault(x => x.Name == field.LookupType);
                        var primaryField = new Field();

                        if (lookupModule != null)
                            primaryField = lookupModule.Fields.Single(x => x.Primary);
                        else
                            continue;

                        if (!field.Name.Contains(".") && isViewFields)
                            dr[i] = record[field.Name];

                        else
                            dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];

                    }
                }

                dt.Rows.Add(dr);
            }

            worksheetData.Cells.ImportDataTable(dt, true, "A1");

            worksheetData.AutoFitColumns();
            worksheetData.AutoFitRows();

            Stream memory = new MemoryStream();

            var fileName = nameModule + ".xlsx";

            workbook.Save(memory, Aspose.Cells.SaveFormat.Xlsx);
            memory.Position = 0;

            return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
        }

        [Route("export_excel_no_data")]
        public async Task<FileStreamResult> ExportExcelNoData(string module, int viewId, int templateId, string templateName, string locale = "", bool? normalize = false, int? timezoneOffset = 180, string listFindRequestJson = "")
        {
            if (string.IsNullOrWhiteSpace(module) || templateId == null || templateId == 0)
            {
                throw new HttpRequestException("Module field is required");
            }

            var moduleEntity = await _moduleRepository.GetByName(module);
            var Module = await _moduleRepository.GetByName(module);
            var template = await _templateRepository.GetById(templateId);
            var fields = Module.Fields.OrderBy(x => x.Id).ToList();
            //var tempsName = templateName;
            //byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(tempsName);
            //var tempName = System.Text.Encoding.ASCII.GetString(bytes);
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository, tenantLanguage: AppUser.TenantLanguage);
            var currentCulture = locale == "en" ? "en-US" : "tr-TR";
            var formatDate = currentCulture == "tr-TR" ? "dd.MM.yyyy" : "M/d/yyyy";
            var formatDateTime = currentCulture == "tr-TR" ? "dd.MM.yyyy HH:mm" : "M/d/yyyy h:mm a";
            var formatTime = currentCulture == "tr-TR" ? "HH:mm" : "h:mm a";
            var format = "";

            var view = await _viewRepository.GetById(viewId);

            if (view == null)
                return null;

            /**
                                * listFindRequestJson View'den gelen data
                                * listFindRequest View'den gelen datayı Json formatına çeviriyoruz
                                */
            FindRequest listFindRequest = null;

            if (!string.IsNullOrWhiteSpace(listFindRequestJson))
            {
                var serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();
                listFindRequest = JsonConvert.DeserializeObject<FindRequest>(listFindRequestJson, serializerSettings);
            }

            var findRequest = new FindRequest();
            var newSortField = "id";
            var newSortDirection = SortDirection.Desc;

            if (listFindRequest.SortField != null && listFindRequest.SortDirection != 0)
            {
                newSortField = listFindRequest.SortField;
                newSortDirection = listFindRequest.SortDirection;
            }

            findRequest.Fields = new List<string>();
            findRequest.SortField = newSortField;
            findRequest.SortDirection = newSortDirection;
            findRequest.Limit = 9999;

            if (listFindRequest.Filters != null && listFindRequest.Filters.Count > 0)
            {

                findRequest.Filters = new List<Filter>();

                foreach (var viewFilter in listFindRequest.Filters)
                {
                    findRequest.Filters.Add(new Filter
                    {
                        Field = viewFilter.Field,
                        Operator = viewFilter.Operator,
                        Value = viewFilter.Value,
                        No = viewFilter.No
                    });
                }
            }
            else if (view.Filters != null && view.Filters.Count > 0)
            {
                findRequest.Filters = new List<Filter>();

                foreach (var viewFilter in view.Filters)
                {
                    viewFilter.Value = viewFilter.Value.Replace("[me]", AppUser.TenantId.ToString());
                    viewFilter.Value = viewFilter.Value.Replace("[me.email]", AppUser.Email);

                    findRequest.Filters.Add(new Filter
                    {
                        Field = viewFilter.Field,
                        Operator = viewFilter.Operator,
                        Value = viewFilter.Value,
                        No = viewFilter.No
                    });
                }
            }

            if (!string.IsNullOrEmpty(view.FilterLogic))
                findRequest.FilterLogic = view.FilterLogic;

            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                if (field.DataType != Model.Enums.DataType.Lookup)
                {
                    findRequest.Fields.Add(field.Name);
                }
                else
                {
                    var lookupModule = lookupModules.FirstOrDefault(x => x.Name == field.LookupType);
                    var primaryField = new Field();

                    if (lookupModule != null)
                        primaryField = lookupModule.Fields.Single(x => x.Primary);
                    else
                        continue;

                    findRequest.Fields.Add(field.Name + "." + field.LookupType + "." + primaryField.Name);
                }
            }

            var records = _recordRepository.Find(moduleEntity.Name, findRequest);

            using (var temp = await _storage.Client.GetObjectStreamAsync(UnifiedStorage.GetPath("template", AppUser.TenantId), template.Content, null))
            {
                Workbook workbook = new Workbook(temp);
                Worksheet worksheetReportAdd = workbook.Worksheets.Add("Report");
                Worksheet worksheetData = workbook.Worksheets[0];
                Worksheet worksheetReportFormul = workbook.Worksheets[1];
                Worksheet worksheetReport = workbook.Worksheets["Report"];
                var row = worksheetReportFormul.Cells.MaxDisplayRange.RowCount + 1;
                var col = worksheetReportFormul.Cells.MaxDisplayRange.ColumnCount + 1;
                var count = worksheetData.Cells.MaxRow;

                worksheetData.Cells.DeleteRows(0, count + 1);

                DataTable dt = new DataTable("Excel");

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];

                    if (!dt.Columns.Contains(field.LabelTr))
                    {
                        switch (field.DataType)
                        {
                            case DataType.Number:
                            case DataType.NumberAuto:
                            case DataType.NumberDecimal:
                            case DataType.Currency:
                                format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                                dt.Columns.Add(format).DataType = typeof(decimal);
                                break;
                            case DataType.Lookup:
                                format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                                dt.Columns.Add(format);
                                break;
                            default:
                                format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                                dt.Columns.Add(format).DataType = typeof(string);
                                break;
                        }
                    }
                    else
                        switch (field.DataType)
                        {
                            case DataType.Number:
                            case DataType.NumberAuto:
                            case DataType.NumberDecimal:
                            case DataType.Currency:
                                format = locale.Contains("tr") ? field.LabelTr + " (" + field.Name + ")" : field.LabelEn + " (" + field.Name + ")";
                                dt.Columns.Add(format).DataType = typeof(decimal);
                                break;
                            case DataType.Lookup:
                                format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                                dt.Columns.Add(format);
                                break;
                            default:
                                format = locale.Contains("tr") ? field.LabelTr + " (" + field.Name + ")" : field.LabelEn + " (" + field.Name + ")";
                                dt.Columns.Add(format).DataType = typeof(string);
                                break;
                        }
                }

                for (int j = 0; j < records.Count; j++)
                {
                    var record = records[j];
                    var dr = dt.NewRow();

                    for (int i = 0; i < fields.Count; i++)
                    {
                        var field = fields[i];

                        //Lookuplarda field name boş geliyor.
                        if ((field.DataType.ToString() != "Lookup") && (record[field.Name] == null || record[field.Name].IsNullOrEmpty()))
                        {
                            continue;
                        }


                        switch (field.DataType)
                        {
                            case DataType.Date:
                                record[field.Name] = ((DateTime)record[field.Name]).AddMinutes((int)timezoneOffset).ToString(formatDate);
                                break;
                            case DataType.DateTime:
                                record[field.Name] = ((DateTime)record[field.Name]).AddMinutes((int)timezoneOffset).ToString(formatDateTime);
                                break;
                            case DataType.Time:
                                record[field.Name] = ((DateTime)record[field.Name]).AddMinutes((int)timezoneOffset).ToString(formatTime);
                                break;
                        }


                        if (field.DataType != Model.Enums.DataType.Lookup)
                        {
                            switch (field.DataType)
                            {
                                case DataType.Number:
                                case DataType.NumberAuto:
                                case DataType.NumberDecimal:
                                case DataType.Currency:
                                    dr[i] = (decimal)record[field.Name];
                                    break;
                                case DataType.Tag:
                                case DataType.Multiselect:
                                    var multi = record[field.Name].ToObject<List<string>>();
                                    dr[i] = string.Join("|", multi);
                                    break;
                                default:
                                    dr[i] = record[field.Name];
                                    break;
                            }
                        }
                        else
                        {
                            var lookupModule = lookupModules.FirstOrDefault(x => x.Name == field.LookupType);
                            var primaryField = new Field();

                            if (lookupModule != null)
                                primaryField = lookupModule.Fields.Single(x => x.Primary);
                            else
                                continue;

                            dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];
                        }
                    }
                    dt.Rows.Add(dr);
                }


                worksheetData.Cells.ImportDataTable(dt, true, "A1");

                workbook.CalculateFormula();
                worksheetData.AutoFitColumns();
                worksheetData.AutoFitRows();

                if (row > 0 && col > 0)
                {
                    var fromRange = worksheetReportFormul.Cells.CreateRange(0, 0, row, col);
                    var toRange = worksheetReport.Cells.CreateRange(0, 0, 1, 1);
                    toRange.CopyValue(fromRange);
                }
                workbook.Worksheets.RemoveAt("Data");
                workbook.Worksheets.RemoveAt("Report Formula");

                Stream memory = new MemoryStream();

                var fileName = templateName + ".xlsx";

                workbook.Save(memory, Aspose.Cells.SaveFormat.Xlsx);
                memory.Position = 0;

                return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
        }

        [Route("export_excel_data")]
        public async Task<FileStreamResult> ExportExcelData(string module, int viewId, string templateName, int templateId, string locale = "", bool? normalize = false, int? timezoneOffset = 180, string listFindRequestJson = "")
        {
            if (string.IsNullOrWhiteSpace(module) || templateId == null || templateId == 0)
            {
                throw new HttpRequestException("Module field is required");
            }

            var moduleEntity = await _moduleRepository.GetByName(module);
            var template = await _templateRepository.GetById(templateId);
            var fields = moduleEntity.Fields.OrderBy(x => x.Id).ToList();
            //var tempsName = templateName;
            //byte[] bytes = System.Text.Encoding.GetEncoding("Cyrillic").GetBytes(tempsName);
            //var tempName = System.Text.Encoding.ASCII.GetString(bytes);
            var lookupModules = await Model.Helpers.RecordHelper.GetLookupModules(moduleEntity, _moduleRepository, tenantLanguage: AppUser.TenantLanguage);
            var currentCulture = locale == "en" ? "en-US" : "tr-TR";
            var formatDate = currentCulture == "tr-TR" ? "dd.MM.yyyy" : "M/d/yyyy";
            var formatDateTime = currentCulture == "tr-TR" ? "dd.MM.yyyy HH:mm" : "M/d/yyyy h:mm a";
            var formatTime = currentCulture == "tr-TR" ? "HH:mm" : "h:mm a";
            var format = "";

            var view = await _viewRepository.GetById(viewId);

            if (view == null)
                return null;

            /**
                                * listFindRequestJson View'den gelen data
                                * listFindRequest View'den gelen datayı Json formatına çeviriyoruz
                                */
            FindRequest listFindRequest = null;

            if (!string.IsNullOrWhiteSpace(listFindRequestJson))
            {
                var serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();
                listFindRequest = JsonConvert.DeserializeObject<FindRequest>(listFindRequestJson, serializerSettings);
            }

            var findRequest = new FindRequest();
            var newSortField = "id";
            var newSortDirection = SortDirection.Desc;

            if (listFindRequest.SortField != null && listFindRequest.SortDirection != 0)
            {
                newSortField = listFindRequest.SortField;
                newSortDirection = listFindRequest.SortDirection;
            }

            findRequest.Fields = new List<string>();
            findRequest.SortField = newSortField;
            findRequest.SortDirection = newSortDirection;
            findRequest.Limit = 9999;

            if (listFindRequest.Filters != null && listFindRequest.Filters.Count > 0)
            {

                findRequest.Filters = new List<Filter>();

                foreach (var viewFilter in listFindRequest.Filters)
                {
                    findRequest.Filters.Add(new Filter
                    {
                        Field = viewFilter.Field,
                        Operator = viewFilter.Operator,
                        Value = viewFilter.Value,
                        No = viewFilter.No
                    });
                }
            }
            else if (view.Filters != null && view.Filters.Count > 0)
            {
                findRequest.Filters = new List<Filter>();

                foreach (var viewFilter in view.Filters)
                {
                    viewFilter.Value = viewFilter.Value.Replace("[me]", AppUser.TenantId.ToString());
                    viewFilter.Value = viewFilter.Value.Replace("[me.email]", AppUser.Email);

                    findRequest.Filters.Add(new Filter
                    {
                        Field = viewFilter.Field,
                        Operator = viewFilter.Operator,
                        Value = viewFilter.Value,
                        No = viewFilter.No
                    });
                }
            }

            if (!string.IsNullOrEmpty(view.FilterLogic))
                findRequest.FilterLogic = view.FilterLogic;


            for (int i = 0; i < fields.Count; i++)
            {
                var field = fields[i];

                if (field.DataType != Model.Enums.DataType.Lookup)
                {
                    findRequest.Fields.Add(field.Name);
                }
                else
                {
                    var lookupModule = lookupModules.FirstOrDefault(x => x.Name == field.LookupType);
                    var primaryField = new Field();

                    if (lookupModule != null)
                        primaryField = lookupModule.Fields.Single(x => x.Primary);
                    else
                        continue;

                    findRequest.Fields.Add(field.Name + "." + field.LookupType + "." + primaryField.Name);
                }
            }

            var records = _recordRepository.Find(moduleEntity.Name, findRequest);

            using (var temp = await _storage.Client.GetObjectStreamAsync(UnifiedStorage.GetPath("template", AppUser.TenantId), template.Content, null))
            {
                Workbook workbook = new Workbook(temp);
                Worksheet worksheetReportAdd = workbook.Worksheets.Add("Report");
                Worksheet worksheetData = workbook.Worksheets[0];
                Worksheet worksheetReportFormul = workbook.Worksheets[1];
                Worksheet worksheetReport = workbook.Worksheets["Report"];
                var row = worksheetReportFormul.Cells.MaxDisplayRange.RowCount + 1;
                var col = worksheetReportFormul.Cells.MaxDisplayRange.ColumnCount + 1;
                var count = worksheetData.Cells.MaxRow;

                worksheetData.Cells.DeleteRows(0, count + 1);

                DataTable dt = new DataTable("Excel");

                for (int i = 0; i < fields.Count; i++)
                {
                    var field = fields[i];

                    if (!dt.Columns.Contains(field.LabelTr))
                    {
                        switch (field.DataType)
                        {
                            case DataType.Number:
                            case DataType.NumberAuto:
                            case DataType.NumberDecimal:
                            case DataType.Currency:
                                format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                                dt.Columns.Add(format).DataType = typeof(decimal);
                                break;
                            case DataType.Lookup:
                                format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                                dt.Columns.Add(format);
                                break;
                            default:
                                format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                                dt.Columns.Add(format).DataType = typeof(string);
                                break;
                        }

                    }
                    else
                        switch (field.DataType)
                        {
                            case DataType.Number:
                            case DataType.NumberAuto:
                            case DataType.NumberDecimal:
                            case DataType.Currency:
                                format = locale.Contains("tr") ? field.LabelTr + " (" + field.Name + ")" : field.LabelEn + " (" + field.Name + ")";
                                dt.Columns.Add(format).DataType = typeof(decimal);
                                break;
                            case DataType.Lookup:
                                format = locale.Contains("tr") ? field.LabelTr : field.LabelEn;
                                dt.Columns.Add(format);
                                break;
                            default:
                                format = locale.Contains("tr") ? field.LabelTr + " (" + field.Name + ")" : field.LabelEn + " (" + field.Name + ")";
                                dt.Columns.Add(format).DataType = typeof(string);
                                break;
                        }
                }

                int recordCount = records.Count;
                for (int j = 0; j < records.Count; j++)
                {
                    var record = records[j];
                    var dr = dt.NewRow();

                    for (int i = 0; i < fields.Count; i++)
                    {
                        var field = fields[i];

                        //Lookuplarda field name boş geliyor.
                        if ((field.DataType.ToString() != "Lookup") && (record[field.Name] == null || record[field.Name].IsNullOrEmpty()))
                        {
                            continue;
                        }

                        switch (field.DataType)
                        {
                            case DataType.Date:
                                record[field.Name] = ((DateTime)record[field.Name]).AddMinutes((int)timezoneOffset).ToString(formatDate);
                                break;
                            case DataType.DateTime:
                                record[field.Name] = ((DateTime)record[field.Name]).AddMinutes((int)timezoneOffset).ToString(formatDateTime);
                                break;
                            case DataType.Time:
                                record[field.Name] = ((DateTime)record[field.Name]).AddMinutes((int)timezoneOffset).ToString(formatTime);
                                break;
                        }


                        if (field.DataType != Model.Enums.DataType.Lookup)
                        {
                            switch (field.DataType)
                            {
                                case DataType.Number:
                                case DataType.NumberAuto:
                                case DataType.NumberDecimal:
                                case DataType.Currency:
                                    dr[i] = (decimal)record[field.Name];
                                    break;
                                case DataType.Tag:
                                case DataType.Multiselect:
                                    var multi = record[field.Name].ToObject<List<string>>();
                                    dr[i] = string.Join("|", multi);
                                    break;
                                default:
                                    dr[i] = record[field.Name];
                                    break;
                            }
                        }
                        else
                        {
                            var lookupModule = lookupModules.FirstOrDefault(x => x.Name == field.LookupType);
                            var primaryField = new Field();

                            if (lookupModule != null)
                                primaryField = lookupModule.Fields.Single(x => x.Primary);
                            else
                                continue;

                            dr[i] = record[field.Name + "." + field.LookupType + "." + primaryField.Name];
                        }
                    }
                    dt.Rows.Add(dr);
                }

                worksheetData.Cells.ImportDataTable(dt, true, "A1");

                workbook.CalculateFormula();
                worksheetData.AutoFitColumns();
                worksheetData.AutoFitRows();

                if (row > 0 && col > 0)
                {
                    var fromRange = worksheetReportFormul.Cells.CreateRange(0, 0, row, col);
                    var toRange = worksheetReport.Cells.CreateRange(0, 0, 1, 1);
                    toRange.CopyValue(fromRange);
                }

                Stream memory = new MemoryStream();

                var fileName = templateName + ".xlsx";

                workbook.Save(memory, Aspose.Cells.SaveFormat.Xlsx);
                memory.Position = 0;

                return File(memory, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);

            }
        }

        private bool HasFieldPermission(int profileId, Field field, Module moduleEntity)
        {
            //Field Permission Control
            if (field.Permissions.Count > 0)
            {
                foreach (var permission in field.Permissions)
                {
                    if (profileId == permission.ProfileId && permission.Type == FieldPermissionType.None)
                        return false;

                }
            }

            //Field Display Detail , Form , List Control
            if (field.DisplayDetail == false && field.DisplayForm == false && field.DisplayList == false)
                return false;

            //Section Permission Control
            var section = moduleEntity.Sections.Single(x => x.Name == field.Section);

            if (section != null && section.Permissions.Count > 0)
            {
                foreach (var sectionPermission in section.Permissions)
                {
                    if (profileId == sectionPermission.ProfileId && sectionPermission.Type == SectionPermissionType.None)
                        return false;
                }
            }

            return true;
        }
    }
}

