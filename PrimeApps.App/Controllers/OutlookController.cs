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
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using Newtonsoft.Json;
using PrimeApps.Model.Common.Record;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Configuration;

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
        private Warehouse _warehouse;
        private IConfiguration _configuration;

        public OutlookController(ISettingRepository settingRepository, IModuleRepository moduleRepository, IViewRepository viewRepository, IProfileRepository profileRepository, IPicklistRepository picklistRepository, IRecordRepository recordRepository, Warehouse warehouse, IMenuRepository menuRepository, IConfiguration configuration)
        {
            _settingRepository = settingRepository;
            _moduleRepository = moduleRepository;
            _viewRepository = viewRepository;
            _profileRepository = profileRepository;
            _picklistRepository = picklistRepository;
            _recordRepository = recordRepository;
            _warehouse = warehouse;
            _menuRepository = menuRepository;
            _configuration = configuration;
        }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            SetContext(context);
            SetCurrentUser(_settingRepository);
            SetCurrentUser(_moduleRepository);
            SetCurrentUser(_viewRepository);
            SetCurrentUser(_profileRepository);
            SetCurrentUser(_picklistRepository);
            SetCurrentUser(_recordRepository);

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
                    DisplayFieldsArray = new[] { "subject", "sender", "recipients", "sending_date", "body", "created_at" }
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
            //TODO Change
            var moduleController = new ModuleController(_moduleRepository, _viewRepository, _profileRepository, _settingRepository, _warehouse, _menuRepository, _configuration)
            {
                /*Request = new HttpRequestMessage(HttpMethod.Post, new Uri(Request.GetDisplayUrl()).AbsoluteUri.Replace("/api/outlook/create_mail_module", "/api/module/create"))*/
            };

            /*moduleController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
            moduleController.Configuration.Formatters.Clear();
            moduleController.Configuration.Formatters.Add(new JsonMediaTypeFormatter { SerializerSettings = serializerSettings });
            moduleController.Configuration.Services.Replace(typeof(IHttpActionSelector), new SnakeCaseActionSelector());
			*/

            return await moduleController.Create(module);
        }

        [Route("create"), HttpPost]
        public async Task<IActionResult> Create([FromBody]JObject mail)
        {
            var outlookModuleSetting = await _settingRepository.GetByKeyAsync("outlook_module");
            var outlookEmailFieldSetting = await _settingRepository.GetByKeyAsync("outlook_email_field");

            if (outlookModuleSetting == null || outlookEmailFieldSetting == null)
                return StatusCode(HttpStatusCode.Status400BadRequest, new { code = "settings_not_found", message = "Outlook settings not found!" });

            var findRequest = new FindRequest
            {
                Filters = new List<Filter> {new Filter
                {
                    Field = outlookEmailFieldSetting.Value,
                    Operator = Operator.Is,
                    Value = (string)mail["sender"],
                    No = 1
                }}
            };

            var records = _recordRepository.Find(outlookModuleSetting.Value, findRequest);

            if (records.IsNullOrEmpty())
                return StatusCode(HttpStatusCode.Status400BadRequest, new { code = "record_not_found", message = "Record not found!" });

            if (records.Count > 1)
                return StatusCode(HttpStatusCode.Status400BadRequest, new { code = "too_many_records", message = "Too many records!" });

            var module = await _moduleRepository.GetByName(outlookModuleSetting.Value);
            mail["owner"] = AppUser.Id;
            mail["related_module"] = 900000 + module.Id;
            mail["related_to"] = records[0]["id"];

            var serializerSettings = JsonHelper.GetDefaultJsonSerializerSettings();

            //TODO Change
            var recordController = new RecordController(_recordRepository, _moduleRepository, _picklistRepository, _warehouse, _configuration)
            {
                /*Request = new HttpRequestMessage(HttpMethod.Post,
	                new Uri(Request.GetDisplayUrl()).AbsoluteUri.Replace("/api/outlook/create", "/api/record/create"))*/
            };

            /*recordController.Request.Properties[HttpPropertyKeys.HttpConfigurationKey] = new HttpConfiguration();
            recordController.Configuration.Formatters.Clear();
            recordController.Configuration.Formatters.Add(new JsonMediaTypeFormatter { SerializerSettings = serializerSettings });
            recordController.Configuration.Services.Replace(typeof(IHttpActionSelector), new SnakeCaseActionSelector());*/

            return await recordController.Create("mails", mail);
        }
    }
}
