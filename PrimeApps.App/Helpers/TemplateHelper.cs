using PrimeApps.App.Models;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PrimeApps.App.Helpers
{
    public static class TemplateHelper
    {
        public static async Task<Template> CreateEntity(TemplateBindingModel templateModel, IUserRepository userRepository)
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

            await CreateTemplateRelations(templateModel, template, userRepository);

            return template;
        }

        public static async Task<Template> CreateEntityExcel(TemplateBindingModel templateModel, IUserRepository userRepository)
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

            await CreateTemplateRelations(templateModel, template, userRepository);

            return template;
        }

        public static async Task UpdateEntity(TemplateBindingModel templateModel, Template template, IUserRepository userRepository)
        {
            template.Name = templateModel.Name;
            template.Subject = templateModel.Subject;
            template.Content = templateModel.Content;
            template.Language = templateModel.Language;
            template.SharingType = templateModel.SharingType;
            template.Active = templateModel.Active;
            template.UpdatedAt = DateTime.UtcNow;

            if (templateModel.Permissions != null && templateModel.Permissions.Count > 0)
            {
                //New Permissions
                foreach (var permissionModel in templateModel.Permissions)
                {
                    if (!permissionModel.Id.HasValue)
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
                        var permissionModel = templateModel.Permissions.FirstOrDefault(x => x.Id == permissionEntity.Id);

                        if (permissionModel == null)
                            continue;

                        permissionEntity.Type = permissionModel.Type;
                    }
                }
            }

            await CreateTemplateRelations(templateModel, template, userRepository);
        }

        private static async Task CreateTemplateRelations(TemplateBindingModel templateModel, Template template, IUserRepository userRepository)
        {
            if (templateModel.Shares != null && templateModel.Shares.Count > 0)
            {
                template.Shares = new List<TemplateShares>();

                foreach (var userId in templateModel.Shares)
                {
                    var sharedUser = await userRepository.GetById(userId);

                    if (sharedUser != null)
                        template.Shares.Add(sharedUser.SharedTemplates.FirstOrDefault(x => x.UserId == userId && x.TemplateId == template.Id));
                }
            }
        }
    }
}