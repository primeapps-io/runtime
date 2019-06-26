using Newtonsoft.Json.Linq;
using PrimeApps.App.Models;
using PrimeApps.Model.Common.ActionButton;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace PrimeApps.App.Helpers
{
    public interface IActionButtonHelper
    {
        Task<bool> ProcessScriptFiles(ICollection<ActionButtonViewModel> actionButtons, IComponentRepository componentRepository);
    }

    public class ActionButtonHelper : IActionButtonHelper
    {
        private IModuleHelper _moduleHelper;

        public ActionButtonHelper(IModuleHelper moduleHelper)
        {
            _moduleHelper = moduleHelper;
        }

        public async Task<bool> ProcessScriptFiles(ICollection<ActionButtonViewModel> actionButtons, IComponentRepository componentRepository)
        {
            var globalConfig = await _moduleHelper.GetGlobalConfig(componentRepository);
            var appConfigs = globalConfig != null && !globalConfig["configs"].IsNullOrEmpty() ? (JObject)globalConfig["configs"] : null;

            foreach (var actionButton in actionButtons)
            {
                if (actionButton.ActionType != ActionButtonEnum.ActionType.Scripting)
                    continue;

                actionButton.Template = _moduleHelper.ReplaceDynamicValues(actionButton.Template, appConfigs);

                if (actionButton.Template.StartsWith("http"))
                {
                    if (!_moduleHelper.IsTrustedUrl(actionButton.Template, globalConfig))
                    {
                        actionButton.Template = "console.error('" + actionButton.Template + " is not a trusted url.');";
                        continue;
                    }

                    try
                    {
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.DefaultRequestHeaders.Accept.Clear();
                            var response = await httpClient.GetAsync(actionButton.Template);
                            var content = await response.Content.ReadAsStringAsync();

                            if (!response.IsSuccessStatusCode)
                            {
                                actionButton.Template = "console.error('" + actionButton.Template + " response error. Http Status Code: " + response.StatusCode + "');";
                                continue;
                            }

                            if (string.IsNullOrWhiteSpace(content))
                            {
                                actionButton.Template = "console.warn('" + actionButton.Template + " has empty content.');";
                                continue;
                            }

                            actionButton.Template = content;
                        }
                    }
                    catch
                    {
                        actionButton.Template = "console.error('" + actionButton.Template + " has connection error. Please check the url.');";
                        continue;
                    }
                }
            }

            return true;
        }
    }
}