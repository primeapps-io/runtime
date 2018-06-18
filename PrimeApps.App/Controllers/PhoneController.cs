using Newtonsoft.Json.Linq;
using PrimeApps.App.Helpers;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PrimeApps.Model.Common.Phone;
using PrimeApps.Model.Enums;
using HttpStatusCode = Microsoft.AspNetCore.Http.StatusCodes;
using Microsoft.AspNetCore.Mvc.Filters;

namespace PrimeApps.App.Controllers
{
    [Route("api/phone")]
    [Authorize]
    public class PhoneController : BaseController
    {
        private ISettingRepository _settingRepository;

        public PhoneController(ISettingRepository settingRepository)
        {
            _settingRepository = settingRepository;
        }

		public override void OnActionExecuting(ActionExecutingContext context)
		{
			SetContext(context);
			SetCurrentUser(_settingRepository);

			base.OnActionExecuting(context);
		}

		[Route("save_provider"), HttpPost]
        public async Task<IActionResult> SaveProvider([FromBody]SipProvider sipProvider)
        {
            //Delete provider if exists
            await _settingRepository.DeleteAsync(Model.Enums.SettingType.Phone, "provider");
            await _settingRepository.DeleteAsync(Model.Enums.SettingType.Phone, "sip_company_key");

            if (sipProvider != null && !string.IsNullOrEmpty(sipProvider.Provider))
            {
                IList<Setting> settings = new List<Setting>();
                settings.Add(new Setting()
                {
                    Deleted = false,
                    Key = "provider",
                    Type = Model.Enums.SettingType.Phone,
                    UserId = null,
                    Value = sipProvider.Provider != null ? sipProvider.Provider : null
                });

                settings.Add(new Setting()
                {
                    Deleted = false,
                    Key = "sip_company_key",
                    Type = Model.Enums.SettingType.Phone,
                    UserId = null,
                    Value = sipProvider.CompanyKey != null ? sipProvider.CompanyKey : null
                });

                var resultCount = await _settingRepository.AddSettings(settings);
                if (resultCount > 0)
                {
                    return Ok();
                }
            }

            return BadRequest();

        }
        [Route("delete_all_settings"), HttpPost]
        public async Task<IActionResult> DeleteAllSettings()
        {
            await _settingRepository.DeleteAsync(Model.Enums.SettingType.Phone);
            return Ok();
        }
        [Route("delete_sip_account/{userId:int}"), HttpDelete]
        public async Task<IActionResult> DeleteSipAccount(int userId)
        {
            await _settingRepository.DeleteAsync(Model.Enums.SettingType.Phone, userId);
            return Ok();
        }
        [Route("save_sip_account"), HttpPost]
        public async Task<IActionResult> SaveSipAccount([FromBody]NewSipAccount sipAccount)
        {
            await _settingRepository.DeleteAsync(SettingType.Phone, sipAccount.UserId);
            List<Setting> settings = new List<Setting>();

            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "sipuri",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = SipProviderHelper.GetProviderSpecificSipUriString(sipAccount.Connector, sipAccount.CompanyKey),
            });
            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "extension",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = sipAccount.Extension,
            });
            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "password",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = sipAccount.Password,
            });
            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "server",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = SipProviderHelper.GetProviderServerConnectionString(sipAccount.Connector)
            });
            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "userId",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = sipAccount.UserId.ToString()
            });
            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "isActive",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = "true"
            });
            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "isAutoRegister",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = sipAccount.IsAutoRegister == true ? "true" : "false"
            });

            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "isAutoRecordDetail",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = sipAccount.IsAutoRecordDetail == true ? "true" : "false"
            });


            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "recordDetailModuleName",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = sipAccount.RecordDetailModuleName
            });

            settings.Add(new Setting()
            {
                Deleted = false,
                Key = "recordDetailPhoneFieldName",
                Type = Model.Enums.SettingType.Phone,
                UserId = sipAccount.UserId,
                Value = sipAccount.RecordDetailPhoneFieldName
            });

            var resultCount = await _settingRepository.AddSettings(settings);
            if (resultCount > 0)
            {
                return Ok();
            }
            return BadRequest();


        }
        [Route("get_config")]
        public async Task<IActionResult> GetConfigAndAccounts()
        {
            var config = new JObject();
            var phoneSettings = _settingRepository.Get(SettingType.Phone);

            if (phoneSettings != null && phoneSettings.Count > 0)
            {
                var licenseCount = phoneSettings.FirstOrDefault(x => x.Key == "sip_license_count");

                if (licenseCount == null)
                    return StatusCode(HttpStatusCode.Status403Forbidden);

                var provider = phoneSettings.FirstOrDefault(r => r.Key == "provider");
                var companyKey = phoneSettings.FirstOrDefault(r => r.Key == "sip_company_key");
                config["sipLicenseCount"] = licenseCount.Value;

                if (provider != null)
                    config["provider"] = provider.Value;

                if (companyKey != null)
                    config["sipCompanyKey"] = companyKey.Value;

                var sipUsers = phoneSettings.Where(x => x.UserId != null).GroupBy(r => r.UserId).ToList();

                if (sipUsers.Count > 0)
                {
                    var sipUserArray = new JArray();
                    foreach (var sipUser in sipUsers)
                    {
                        var data = new JObject();

                        foreach (var user in sipUser)
                        {
                            if (user.Key != "password")
                            {
                                data.Add(user.Key, user.Value);
                            }
                        }

                        sipUserArray.Add(data);
                    }

                    config["sipUsers"] = sipUserArray;
                }

                return Ok(config);
            }

            return Ok();
        }
        [Route("get_sip_password")]
        public async Task<IActionResult> GetSipPassword()
        {
            var userId = AppUser.Id;
            var userSettings = await _settingRepository.GetAsync(Model.Enums.SettingType.Phone, userId);
            string password = null;
            if (userSettings != null)
            {
                password = userSettings.FirstOrDefault(r => r.Key == "password").Value;
                if (password != null)
                {
                    return Ok(password);
                }
            }
            return Ok();
        }

    }
}
