using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using PrimeApps.App.ActionFilters;
using PrimeApps.App.Helpers;
using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using Newtonsoft.Json;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;
using Npgsql;
using PrimeApps.Model.Helpers.QueryTranslation;
using ModuleHelper = PrimeApps.Model.Helpers.ModuleHelper;

namespace PrimeApps.App.Controllers
{
    [Route("api/outlook"), Authorize]
    public class OutlookController : ApiBaseController
    {
        private ISettingRepository _settingRepository;
        private IModuleRepository _moduleRepository;
        private IViewRepository _viewRepository;
        private IProfileRepository _profileRepository;
        private IPicklistRepository _picklistRepository;
        private IRecordRepository _recordRepository;
        private IMenuRepository _menuRepository;
        private ITagRepository _tagRepository;
        private Warehouse _warehouse;
        private IConfiguration _configuration;

        private IRecordHelper _recordHelper;
        private IModuleHelper _moduleHelper;

        public OutlookController(ISettingRepository settingRepository, IModuleRepository moduleRepository, IViewRepository viewRepository, IProfileRepository profileRepository, IPicklistRepository picklistRepository, IRecordRepository recordRepository, IMenuRepository menuRepository, ITagRepository tagRepository, Warehouse warehouse, IRecordHelper recordHelper, IModuleHelper moduleHelper, IConfiguration configuration)
        {
            _settingRepository = settingRepository;
            _moduleRepository = moduleRepository;
            _viewRepository = viewRepository;
            _profileRepository = profileRepository;
            _picklistRepository = picklistRepository;
            _recordRepository = recordRepository;
            _tagRepository = tagRepository;
            _menuRepository = menuRepository;
            _warehouse = warehouse;

            _recordHelper = recordHelper;
            _moduleHelper = moduleHelper;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_settingRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_moduleRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_viewRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_profileRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_picklistRepository, PreviewMode, TenantId, AppId);
            SetCurrentUser(_recordRepository, PreviewMode, TenantId, AppId);

            base.OnActionExecuting(context);
        }

        [Route("get_settings"), HttpGet]
        public async Task<IActionResult> GetSettings()
        {
            List<Setting> outlookSettings = null;
            var outlookModuleSetting = await _settingRepository.GetByKeyAsync("outlook_module");
            var outlookEmailFieldSetting = await _settingRepository.GetByKeyAsync("outlook_email_field");

            if (outlookModuleSetting != null && outlookEmailFieldSetting != null)
            {
                outlookSettings = new List<Setting>
                {
                    outlookModuleSetting,
                    outlookEmailFieldSetting
                };
            }

            return Ok(outlookSettings);
        }

        [Route("save_settings"), HttpPost]
        public async Task<IActionResult> SaveSettings([FromBody]OutlookBindingModel outlookSetting)
        {
            var outlookModuleSetting = await _settingRepository.GetByKeyAsync("outlook_module");
            var outlookEmailFieldSetting = await _settingRepository.GetByKeyAsync("outlook_email_field");

            if (outlookModuleSetting == null)
            {
                outlookModuleSetting = new Setting();
                outlookModuleSetting.Type = SettingType.Outlook;
                outlookModuleSetting.Key = "outlook_module";
                outlookModuleSetting.Value = outlookSetting.Module;

                var result = await _settingRepository.Create(outlookModuleSetting);

                if (result < 1)
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
            }
            else
            {
                outlookModuleSetting.Value = outlookSetting.Module;

                await _settingRepository.Update(outlookModuleSetting);
            }

            if (outlookEmailFieldSetting == null)
            {
                outlookEmailFieldSetting = new Setting();
                outlookEmailFieldSetting.Type = SettingType.Outlook;
                outlookEmailFieldSetting.Key = "outlook_email_field";
                outlookEmailFieldSetting.Value = outlookSetting.EmailField;

                var result = await _settingRepository.Create(outlookEmailFieldSetting);

                if (result < 1)
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
            }
            else
            {
                outlookEmailFieldSetting.Value = outlookSetting.EmailField;

                await _settingRepository.Update(outlookEmailFieldSetting);
            }

            //Save module relation
            var module = await _moduleRepository.GetByName(outlookSetting.Module);
            var relation = module.Relations.FirstOrDefault(x => x.RelatedModule == "mails" && !x.Deleted);

            if (relation == null)
            {
                relation = new Relation
                {
                    ModuleId = module.Id,
                    RelatedModule = "mails",
                    RelationType = RelationType.OneToMany,
                    RelationField = "related_to",
                    LabelEnPlural = "Mail",
                    LabelEnSingular = "Mails",
                    LabelTrPlural = "E-Postalar",
                    LabelTrSingular = "E-Posta",
                    Order = (short)(module.Relations.Count + 1),
                    DisplayFieldsArray = new[] {"subject", "sender", "recipients", "sending_date", "body", "created_at", "state"}
                };

                var resultCreate = await _moduleRepository.CreateRelation(relation);

                if (resultCreate < 1)
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                //throw new HttpResponseException(HttpStatusCode.InternalServerError);
            }

            return Ok();
        }

        [Route("create_mail_module"), HttpPost]
        public async Task<IActionResult> CreateMailModule()
        {
            var picklist = new PicklistBindingModel {LabelTr = "Mail Yönü", LabelEn = "Mail Direction", Items = new List<PicklistItemBindingModel>()};
            picklist.Items.Add(new PicklistItemBindingModel {LabelTr = "Giden E-posta", LabelEn = "Out", Value = "out", Order = 1});
            picklist.Items.Add(new PicklistItemBindingModel {LabelTr = "Gelen E-posta", LabelEn = "In", Value = "in", Order = 2});

            var picklistEntity = PicklistHelper.CreateEntity(picklist);
            var result = await _picklistRepository.Create(picklistEntity);

            if (result < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            var moduleJson = @"{
                                'display': false,
                                'fields': [
                                    {
                                        'data_type': 'lookup',
                                        'display_detail': true,
                                        'display_form': true,
                                        'display_list': true,
                                        'inline_edit': true,
                                        'label_en': 'Owner',
                                        'label_tr': 'Kayıt Sahibi',
                                        'lookup_type': 'users',
                                        'name': 'owner',
                                        'order': 1,
                                        'primary': false,
                                        'section': 'mail_information',
                                        'section_column': 1,
                                        'show_label':true,
                                        'validation': {
                                            'readonly': false,
                                            'required': true
                                        }
                                    },
                                    {
                                        'data_type': 'text_single',
                                        'display_detail': true,
                                        'display_form': true,
                                        'display_list': true,
                                        'inline_edit': true,
                                        'label_en': 'Sender',
                                        'label_tr': 'Gönderen',
                                        'name': 'sender',
                                        'order': 2,
                                        'primary': false,
                                        'section': 'mail_information',
                                        'section_column': 1,
                                        'show_label':true,
                                        'validation': {
                                            'readonly': false,
                                            'required': true
                                        }
                                    },
                                    {
                                        'data_type': 'text_single',
                                        'display_detail': true,
                                        'display_form': true,
                                        'display_list': true,
                                        'inline_edit': true,
                                        'label_en': 'Recipient',
                                        'label_tr': 'Alıcı',
                                        'name': 'recipient',
                                        'order': 3,
                                        'primary': false,
                                        'section': 'mail_information',
                                        'section_column': 1,
                                        'show_label':true,
                                        'validation': {
                                            'readonly': false,
                                            'required': true
                                        },
                                    },
                                    {
                                        'data_type': 'date_time',
                                        'display_detail': true,
                                        'display_form': true,
                                        'display_list': true,
                                        'inline_edit': true,
                                        'label_en': 'Sending Date',
                                        'label_tr': 'Gönderilme Tarihi',
                                        'name': 'sending_date',
                                        'order': 4,
                                        'primary': false,
                                        'section': 'mail_information',
                                        'section_column': 1,
                                        'show_label':true,
                                        'validation': {
                                            'readonly': false,
                                            'required': true
                                        },
                                    },
                                    {
                                        'data_type': 'picklist',
                                        'display_detail': true,
                                        'display_form': true,
                                        'display_list': true,
                                        'inline_edit': true,
                                        'label_en': 'Related Module',
                                        'label_tr': 'İlgili Modül',
                                        'name': 'related_module',
                                        'order': 5,
                                        'picklist_id': 25,
                                        'primary': false,
                                        'section': 'mail_information',
                                        'section_column': 2,
                                        'show_label':true,
                                    },
                                    {
                                        'data_type': 'lookup',
                                        'display_detail': true,
                                        'display_form': true,
                                        'display_list': true,
                                        'inline_edit': false,
                                        'label_en': 'Related to',
                                        'label_tr': 'İlgili Kayıt',
                                        'lookup_relation': 'related_module',
                                        'lookup_type': 'relation',
                                        'name': 'related_to',
                                        'order': 6,
                                        'primary': false,
                                        'section': 'mail_information',
                                        'section_column': 2,
                                        'show_label':true,
                                    },
                                    {
                                        'data_type': 'text_single',
                                        'display_detail': true,
                                        'display_form': true,
                                        'display_list': true,
                                        'inline_edit': true,
                                        'label_en': 'Subject',
                                        'label_tr': 'Konu',
                                        'name': 'subject',
                                        'order': 7,
                                        'primary': true,
                                        'section': 'mail_content',
                                        'section_column': 1,
                                        'show_label':true,
                                        'validation': {
                                            'readonly': false,
                                            'required': true
                                        },
                                    },
                                    {
                                        'data_type': 'text_multi',
                                        'display_detail': true,
                                        'display_form': true,
                                        'display_list': true,
                                        'inline_edit': true,
                                        'label_en': 'Body',
                                        'label_tr': 'İçerik',
                                        'name': 'body',
                                        'order': 8,
                                        'primary': false,
                                        'section': 'mail_content',
                                        'section_column': 1,
                                        'show_label':true,
                                        'multiline_type': 'large',
                                        'validation': {
                                            'readonly': false,
                                            'required': true
                                        },
                                    },
                                    {
                                        'data_type': 'lookup',
                                        'display_detail': true,
                                        'display_form': false,
                                        'display_list': true,
                                        'inline_edit': false,
                                        'label_en': 'Created by',
                                        'label_tr': 'Oluşturan',
                                        'lookup_type': 'users',
                                        'name': 'created_by',
                                        'order': 9,
                                        'primary': false,
                                        'section': 'system_information',
                                        'section_column': 1,
                                        'show_label':true,
                                        'validation': {
                                            'readonly': true,
                                            'required': true
                                        },
                                    },
                                    {
                                        'data_type': 'date_time',
                                        'display_detail': true,
                                        'display_form': false,
                                        'display_list': true,
                                        'inline_edit': false,
                                        'label_en': 'Created at',
                                        'label_tr': 'Oluşturulma Tarihi',
                                        'name': 'created_at',
                                        'order': 10,
                                        'primary': false,
                                        'section': 'system_information',
                                        'section_column': 1,
                                        'show_label':true,
                                        'validation': {
                                            'readonly': true,
                                            'required': true
                                        },
                                    },
                                    {
                                        'data_type': 'lookup',
                                        'display_detail': true,
                                        'display_form': false,
                                        'display_list': true,
                                        'inline_edit': false,
                                        'label_en': 'Updated by',
                                        'label_tr': 'Güncelleyen',
                                        'lookup_type': 'users',
                                        'name': 'updated_by',
                                        'order': 11,
                                        'primary': false,
                                        'section': 'system_information',
                                        'section_column': 2,
                                        'show_label':true,
                                        'validation': {
                                            'readonly': true,
                                            'required': true
                                        }
                                    },
                                    {
                                        'data_type': 'date_time',
                                        'display_detail': true,
                                        'display_form': false,
                                        'display_list': true,
                                        'inline_edit': false,
                                        'label_en': 'Updated at',
                                        'label_tr': 'Güncellenme Tarihi',
                                        'name': 'updated_at',
                                        'order': 12,
                                        'primary': false,
                                        'section': 'system_information',
                                        'section_column': 2,
                                        'show_label':true,
                                        'validation': {
                                            'readonly': true,
                                            'required': true
                                        }
                                    },
                                    {
										'data_type': 'picklist',
                                        'display_detail': true,
                                        'display_form': true,
                                        'display_list': true,
                                        'inline_edit': false,
                                        'label_en': 'State',
                                        'label_tr': 'Durum',
                                        'name': 'state',
                                        'order': 13,
                                        'picklist_id':" + picklistEntity.Id + "," +
                             @"'primary': false,
                                        'section': 'mail_information',
                                        'section_column': 2,
                                        'show_label':true,
                                    }
                                ],
                                'label_en_plural': 'Mails',
                                'label_en_singular': 'Mail',
                                'label_tr_plural': 'E-Postalar',
                                'label_tr_singular': 'E-Posta',
                                'menu_icon': 'fa fa-envelope-o',
                                'name': 'mails',
                                'sections': [
                                    {
                                        'column_count': 2,
                                        'display_detail': true,
                                        'display_form': true,
                                        'label_en': 'Mail Information',
                                        'label_tr': 'E-Posta Bilgisi',
                                        'name': 'mail_information',
                                        'order': 1
                                    },
                                    {
                                        'column_count': 1,
                                        'display_detail': true,
                                        'display_form': true,
                                        'label_en': 'Mail Content',
                                        'label_tr': 'E-Posta İçeriği',
                                        'name': 'mail_content',
                                        'order': 2
                                    },
                                    {
                                        'column_count': 2,
                                        'display_detail': true,
                                        'display_form': false,
                                        'label_en': 'System Information',
                                        'label_tr': 'Sistem Bilgisi',
                                        'name': 'system_information',
                                        'order': 3
                                    }
                                ],
                                'sharing': 'private',
                                'order': 99
                            }";

            var serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();
            var module = JsonConvert.DeserializeObject<ModuleBindingModel>(moduleJson, serializerSettings);
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            //Create module
            var moduleEntity = _moduleHelper.CreateEntity(module);
            var resultCreate = await _moduleRepository.Create(moduleEntity);

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());

            //Create default views
            try
            {
                var defaultViewAllRecordsEntity = await ViewHelper.CreateDefaultViewAllRecords(moduleEntity, _moduleRepository, AppUser.TenantLanguage);
                var resultCreateViewAllRecords = await _viewRepository.Create(defaultViewAllRecordsEntity);

                if (resultCreateViewAllRecords < 1)
                {
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                }
            }
            catch (Exception)
            {
                await _moduleRepository.DeleteHard(moduleEntity);
                throw;
            }

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            //Create dynamic table
            try
            {
                var resultCreateTable = await _moduleRepository.CreateTable(moduleEntity, AppUser.TenantLanguage);

                if (resultCreateTable != -1)
                {
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }
            }
            catch (Exception)
            {
                await _moduleRepository.DeleteHard(moduleEntity);
                throw;
            }

            //Create dynamic table indexes
            try
            {
                var resultCreateIndexes = await _moduleRepository.CreateIndexes(moduleEntity);

                if (resultCreateIndexes != -1)
                {
                    await _moduleRepository.DeleteTable(moduleEntity);
                    await _moduleRepository.DeleteHard(moduleEntity);
                    throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
                    //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);
                }
            }
            catch (Exception)
            {
                await _moduleRepository.DeleteTable(moduleEntity);
                await _moduleRepository.DeleteHard(moduleEntity);
                throw;
            }

            //Create default permissions for the new module.
            await _profileRepository.AddModuleAsync(moduleEntity.Id);
            await _menuRepository.AddModuleToMenuAsync(moduleEntity);

            _moduleHelper.AfterCreate(AppUser, moduleEntity);

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/module/get?id=" + moduleEntity.Id, moduleEntity);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]JObject mail)
        {
            var outlookModuleSetting = await _settingRepository.GetByKeyAsync("outlook_module");
            var outlookEmailFieldSetting = await _settingRepository.GetByKeyAsync("outlook_email_field");

            if (outlookModuleSetting == null || outlookEmailFieldSetting == null)
                return StatusCode(HttpStatusCode.Status400BadRequest, new {code = "settings_not_found", message = "Outlook settings not found!"});

            var findRequest = new FindRequest
            {
                Filters = new List<Filter>
                {
                    new Filter
                    {
                        Field = outlookEmailFieldSetting.Value,
                        Operator = Operator.Is,
                        Value = (string)mail["sender"],
                        No = 1
                    },
                    new Filter
                    {
                        Field = outlookEmailFieldSetting.Value,
                        Operator = Operator.Is,
                        Value = (string)mail["recipient"],
                        No = 1
                    }
                }
            };

            findRequest.LogicType = LogicType.Or;

            var records = await _recordRepository.Find(outlookModuleSetting.Value, findRequest);

            if (records.IsNullOrEmpty())
                return StatusCode(HttpStatusCode.Status400BadRequest, new {code = "record_not_found", message = "Record not found!"});

            if (records.Count > 1)
                return StatusCode(HttpStatusCode.Status400BadRequest, new {code = "too_many_records", message = "Too many records!"});

            var module = await _moduleRepository.GetByName(outlookModuleSetting.Value);
            mail["owner"] = AppUser.Id;
            mail["related_module"] = 900000 + module.Id;
            mail["related_to"] = records[0]["id"];

            int timezoneOffset = 180;
            bool? normalize = false;
            string locale = "";
            var serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var moduleEntity = await _moduleRepository.GetByName(module.Name);

            if (moduleEntity == null || mail == null)
                return BadRequest();

            var resultBefore = await _recordHelper.BeforeCreateUpdate(moduleEntity, mail, ModelState, AppUser.TenantLanguage, _moduleRepository, _picklistRepository, _profileRepository, _tagRepository, _settingRepository, appUser: AppUser);

            if (resultBefore < 0 && !ModelState.IsValid)
                return BadRequest(ModelState);

            //Set warehouse database name
            _warehouse.DatabaseName = AppUser.WarehouseDatabaseName;

            int resultCreate;

            try
            {
                resultCreate = await _recordRepository.Create(mail, moduleEntity);

                // If module is opportunities create stage history
                if (module.Name == "opportunities")
                    await _recordHelper.CreateStageHistory(mail);
            }
            catch (PostgresException ex)
            {
                if (ex.SqlState == PostgreSqlStateCodes.UniqueViolation)
                    return StatusCode(HttpStatusCode.Status409Conflict, _recordHelper.PrepareConflictError(ex));

                if (ex.SqlState == PostgreSqlStateCodes.ForeignKeyViolation)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new {message = ex.Detail});

                if (ex.SqlState == PostgreSqlStateCodes.UndefinedColumn)
                    return StatusCode(HttpStatusCode.Status400BadRequest, new {message = ex.MessageText});

                throw;
            }

            if (resultCreate < 1)
                throw new ApplicationException(HttpStatusCode.Status500InternalServerError.ToString());
            //throw new HttpResponseException(HttpStatusCode.Status500InternalServerError);

            //Check number auto fields and combinations and update record with combined values
            var numberAutoFields = moduleEntity.Fields.Where(x => x.DataType == DataType.NumberAuto).ToList();

            if (numberAutoFields.Count > 0)
            {
                var currentRecord = await _recordRepository.GetById(moduleEntity, (int)mail["id"], AppUser.HasAdminProfile);
                var hasUpdate = false;

                foreach (var numberAutoField in numberAutoFields)
                {
                    var combinationFields = moduleEntity.Fields.Where(x => x.Combination != null && (x.Combination.Field1 == numberAutoField.Name || x.Combination.Field2 == numberAutoField.Name)).ToList();

                    if (combinationFields.Count > 0)
                    {
                        foreach (var combinationField in combinationFields)
                        {
                            await _recordHelper.SetCombinations(currentRecord, _moduleRepository, AppUser.Culture, null, combinationField, 180);
                        }

                        hasUpdate = true;
                    }
                }

                if (hasUpdate)
                {
                    var recordUpdate = new JObject();
                    recordUpdate["id"] = (int)mail["id"];

                    var combinationFields = moduleEntity.Fields.Where(x => x.Combination != null).ToList();

                    foreach (var combinationField in combinationFields)
                    {
                        recordUpdate[combinationField.Name] = currentRecord[combinationField.Name];
                    }

                    await _recordRepository.Update(recordUpdate, moduleEntity);
                }
            }

            //After create
            _recordHelper.AfterCreate(moduleEntity, mail, AppUser, _warehouse, timeZoneOffset: timezoneOffset);

            //Format records if has locale
            if (!string.IsNullOrWhiteSpace(locale))
            {
                ICollection<Module> lookupModules = new List<Module> {ModuleHelper.GetFakeUserModule()};
                var currentCulture = locale == "en" ? "en-US" : "tr-TR";
                mail = await _recordRepository.GetById(moduleEntity, (int)mail["id"], !AppUser.HasAdminProfile, lookupModules);
                mail = await Model.Helpers.RecordHelper.FormatRecordValues(moduleEntity, mail, _moduleRepository, _picklistRepository, _configuration, AppUser.TenantGuid, AppUser.TenantLanguage, currentCulture, timezoneOffset, lookupModules);

                if (normalize.HasValue && normalize.Value)
                    mail = Model.Helpers.RecordHelper.NormalizeRecordValues(mail);
            }

            var uri = new Uri(Request.GetDisplayUrl());
            return Created(uri.Scheme + "://" + uri.Authority + "/api/record/get/" + module + "/?id=" + mail["id"], mail);
        }
    }
}