using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Studio.Helpers
{
    public interface IPermissionHelper
    {
        Task<bool> CheckUserRole(int userId, int organizationId, OrganizationRole role);
        bool CheckUserProfile(ProfileEnum profile, string apiUrl, RequestTypeEnum requestType);
    }

    public class PermissionHelper : IPermissionHelper
    {

        private IServiceScopeFactory _serviceScopeFactory;
        private IHttpContextAccessor _context;
        private IOrganizationUserRepository _organizationUserRepository;
        private IConfiguration _configuration;

        public PermissionHelper(IHttpContextAccessor context, IConfiguration configuration, IServiceScopeFactory serviceScopeFactory, IOrganizationUserRepository organizationUserRepository)
        {
            _context = context;
            _configuration = configuration;
            _serviceScopeFactory = serviceScopeFactory;
            _organizationUserRepository = organizationUserRepository;
        }

        public async Task<bool> CheckUserRole(int userId, int organizationId, OrganizationRole role)
        {
            var userRole = await _organizationUserRepository.GetUserRole(userId, organizationId);

            return userRole == role;
        }

        public bool CheckUserProfile(ProfileEnum profile, string apiUrl, RequestTypeEnum requestType)
        {
            bool hasPermission = true;
            switch (apiUrl)
            {
                case "action_button":
                    if (requestType == RequestTypeEnum.View)
                    {
                        if(profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if(profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Create)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Update)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Delete)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    break;
                case "app_collaborator":
                    if (requestType == RequestTypeEnum.View)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Create)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = false;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Update)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = false;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Delete)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = false;
                        }
                    }
                    break;
                case "bpm":
                    if (requestType == RequestTypeEnum.View)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Create)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Update)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Delete)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    break;
                case "component":
                case "deployment_component":
                case "functions":
                case "deployment_function":
                    if (requestType == RequestTypeEnum.View)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Create)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Update)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Delete)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    break;
                case "Document":
                    if (requestType == RequestTypeEnum.View)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Create)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Update)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Delete)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    break;
                case "help":
                    if (requestType == RequestTypeEnum.View)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Create)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Update)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Delete)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    break;
                case "menu":
                    if (requestType == RequestTypeEnum.View)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Create)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Update)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Delete)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    break;
                case "module":
                    if (requestType == RequestTypeEnum.View)
                    {
                        if (profile == ProfileEnum.Manager)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Create)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Update)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Delete)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    break;
                case "module_profile_settings":
                    if (requestType == RequestTypeEnum.View)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Create)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Update)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    else if (requestType == RequestTypeEnum.Delete)
                    {
                        if (profile == ProfileEnum.Viewer)
                        {
                            hasPermission = false;
                        }
                        else if (profile == ProfileEnum.Developer)
                        {
                            hasPermission = true;
                        }
                    }
                    break;
            }

            return hasPermission;
        }
    }
}
