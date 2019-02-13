using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Models;

namespace PrimeApps.Studio.Helpers
{
    public static class HelpHelper
    {
        public static async Task<Help> CreateEntity(HelpBindingModel helpModel, IUserRepository userRepository)
        {
            var help = new Help
            {
                Template = helpModel.Template,
                ModuleId = helpModel.ModuleId,
                RouteUrl = helpModel.RouteUrl,
                FirstScreen = helpModel.FirstScreen,
                ModalType = helpModel.ModalType,
                ShowType = helpModel.ShowType,
                ModuleType = helpModel.ModuleType,
                Name = helpModel.Name,
                CustomHelp = helpModel.CustomHelp
               

            };

            return help;
        }

        public static async Task<Help> UpdateEntity(HelpBindingModel helpModel, Help help, IUserRepository userRepository)
        {
            help.Template = helpModel.Template;
            help.ModuleId = helpModel.ModuleId;
            help.RouteUrl = helpModel.RouteUrl;
            help.FirstScreen = helpModel.FirstScreen;
            help.ModalType = helpModel.ModalType;
            help.ShowType = helpModel.ShowType;
            help.ModuleType = helpModel.ModuleType;
            help.Name = helpModel.Name;
            help.CustomHelp = helpModel.CustomHelp;
           

            return help;
        }



    }
}