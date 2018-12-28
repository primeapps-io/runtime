using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Console.Models;

namespace PrimeApps.Console.Helpers
{
    public class ModuleProfileSettingHelper
    {
        public static ModuleProfileSetting CreateEntity(ModuleProfileSettingBindingModels moduleProfileSettingModel, IModuleProfileSettingRepository moduleProfileSettingRepository)
        {
            var moduleProfileSetting = new ModuleProfileSetting
            {
                ModuleId = moduleProfileSettingModel.ModuleId,
                Profiles = moduleProfileSettingModel.Profiles,
                LabelEnSingular = moduleProfileSettingModel.LabelEnSingular,
                LabelEnPlural = moduleProfileSettingModel.LabelEnPlural,
                LabelTrSingular = moduleProfileSettingModel.LabelTrSingular,
                LabelTrPlural = moduleProfileSettingModel.LabelTrPlural,
                MenuIcon = moduleProfileSettingModel.MenuIcon,
                Display = moduleProfileSettingModel.Display
            };

            return moduleProfileSetting;
        }

        public static ModuleProfileSetting UpdateEntity(ModuleProfileSettingBindingModels moduleProfileSettingModel, ModuleProfileSetting moduleProfileSetting, IModuleProfileSettingRepository moduleProfileSettingRepository)
        {
            moduleProfileSetting.ModuleId = moduleProfileSettingModel.ModuleId;
            moduleProfileSetting.Profiles = moduleProfileSettingModel.Profiles;
            moduleProfileSetting.LabelEnSingular = moduleProfileSettingModel.LabelEnSingular;
            moduleProfileSetting.LabelEnPlural = moduleProfileSettingModel.LabelEnPlural;
            moduleProfileSetting.LabelTrSingular = moduleProfileSettingModel.LabelTrSingular;
            moduleProfileSetting.LabelTrPlural = moduleProfileSettingModel.LabelTrPlural;
            moduleProfileSetting.MenuIcon = moduleProfileSettingModel.MenuIcon;
            moduleProfileSetting.Display = moduleProfileSettingModel.Display;

            return moduleProfileSetting;
        }
    }
}