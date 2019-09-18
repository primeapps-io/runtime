using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Platform;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Studio.Models;

namespace PrimeApps.Studio.Helpers
{
    public static class TemplateHelper
    {
        public static Template CreateEntity(TemplateBindingModel templateModel, IUserRepository userRepository)
        {

            var template = new Template
            {
                Module = templateModel.Module,
                TemplateType = templateModel.TemplateType,
                Name = templateModel.Name,
                Subject = templateModel.Subject,
                Content = templateModel.Content,
                Language = templateModel.Language,
                Active = templateModel.Active,
                SharingType = templateModel.SharingType
            };

            if (templateModel.Permissions != null && templateModel.Permissions.Count > 0)
            {
                template.Permissions = new List<TemplatePermission>();

                foreach (var permissionModel in templateModel.Permissions)
                {
                    var permissionEntity = new TemplatePermission
                    {
                        ProfileId = permissionModel.ProfileId,
                        Type = permissionModel.Type
                    };

                    template.Permissions.Add(permissionEntity);
                }
            }

            CreateTemplateRelations(templateModel, template, userRepository);

            return template;

        }

        public static Template CreateEntityExcel(TemplateBindingModel templateModel, IUserRepository userRepository)
        {
            var template = new Template
            {
                Module = templateModel.Module,
                TemplateType = templateModel.TemplateType,
                Name = templateModel.Name,
                Subject = templateModel.Subject,
                Content = templateModel.Content,
                Language = templateModel.Language,
                Active = templateModel.Active,
                SharingType = templateModel.SharingType
            };

            if (templateModel.Permissions != null && templateModel.Permissions.Count > 0)
            {
                template.Permissions = new List<TemplatePermission>();

                foreach (var permissionModel in templateModel.Permissions)
                {
                    var permissionEntity = new TemplatePermission
                    {
                        ProfileId = permissionModel.ProfileId,
                        Type = permissionModel.Type
                    };

                    template.Permissions.Add(permissionEntity);
                }
            }

            CreateTemplateRelations(templateModel, template, userRepository);

            return template;
        }

        public static void UpdateEntity(TemplateBindingModel templateModel, Template template, IUserRepository userRepository, AppTemplateBindingModel appTemplateModel, AppTemplate appTemplate, bool isAppTemplate = false)
        {
            if (!isAppTemplate)
            {
                template.Name = templateModel.Name;
                template.Subject = templateModel.Subject;
                template.Content = templateModel.Content;
                template.Language = templateModel.Language;
                template.SharingType = templateModel.SharingType;
                template.Active = templateModel.Active;
                template.Module = templateModel.Module;
                template.UpdatedAt = DateTime.UtcNow;

                if (templateModel.Permissions != null && templateModel.Permissions.Count > 0)
                {
                    //New Permissions
                    foreach (var permissionModel in templateModel.Permissions)
                    {

                        if (!permissionModel.Id.HasValue && !template.Permissions.Any(q => q.ProfileId == permissionModel.ProfileId))
                        {
                            if (template.Permissions == null)
                                template.Permissions = new List<TemplatePermission>();

                            var permissionEntity = new TemplatePermission
                            {
                                ProfileId = permissionModel.ProfileId,
                                Type = permissionModel.Type
                            };

                            template.Permissions.Add(permissionEntity);
                        }
                    }

                    //Existing Permissions
                    if (template.Permissions != null && template.Permissions.Count > 0)
                    {
                        foreach (var permissionEntity in template.Permissions)
                        {
                            var result = templateModel.Permissions.FirstOrDefault(x => x.ProfileId == permissionEntity.ProfileId);

                            if (result == null)
                                permissionEntity.Deleted = true;

                            var permissionModel = templateModel.Permissions.FirstOrDefault(x => x.Id == permissionEntity.Id);

                            if (permissionModel == null)
                                continue;

                            permissionEntity.Type = permissionModel.Type;
                        }
                    }
                }

                CreateTemplateRelations(templateModel, template, userRepository);
            }
            else// if (isAppTemplate)
            {
                appTemplate.Name = appTemplateModel.Name;
                appTemplate.Subject = appTemplateModel.Subject;
				appTemplate.Settings = appTemplateModel.Settings;
                appTemplate.Content = appTemplateModel.Content;
                appTemplate.Language = appTemplateModel.Language;
                appTemplate.Active = appTemplateModel.Active;
                appTemplate.UpdatedAt = DateTime.UtcNow;
            }
        }

        private static void CreateTemplateRelations(TemplateBindingModel templateModel, Template template, IUserRepository userRepository)
        {
            if (templateModel.Shares != null && templateModel.Shares.Count > 0)
            {
                template.Shares = new List<TemplateShares>();

                foreach (var userId in templateModel.Shares)
                {
                    var sharedUser = userRepository.GetById(userId);

                    if (sharedUser != null)
                        template.Shares.Add(sharedUser.SharedTemplates.FirstOrDefault(x => x.UserId == userId && x.TemplateId == template.Id));
                }
            }
        }

        public static AppTemplate CreateEntityAppTemplate(AppTemplateBindingModel appTemplate, int? appId)
        {

            var template = new AppTemplate
            {
                Name = appTemplate.Name,
                Subject = appTemplate.Subject,
                Content = appTemplate.Content,
                Language = appTemplate.Language,
                Active = appTemplate.Active,
                Settings = appTemplate.Settings,
                Type = AppTemplateType.Email,
                AppId = (int)appId
            };

            var systemCodes = appTemplate.Name.Split(" ");
            foreach (var systemCode in systemCodes)
            {
                template.SystemCode += systemCode.ToLower() + "_";
            }

            return template;
        }
    }
}