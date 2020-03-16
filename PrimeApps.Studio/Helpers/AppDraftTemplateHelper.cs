using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrimeApps.Model.Common.Cache;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories;
using Sentry.Protocol;

namespace PrimeApps.Studio.Helpers
{
    public interface IAppDraftTemplateHelper
    {
        Task CreateAppDraftTemplates(AppDraft appDraft);
    }

    public class AppDraftTemplateHelper : IAppDraftTemplateHelper
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly IHttpContextAccessor _context;
        private IConfiguration _configuration;
        //private CurrentUser _currentUser;
        
        public AppDraftTemplateHelper(IHttpContextAccessor context, IConfiguration configuration,
            IServiceScopeFactory serviceScopeFactory)
        {
            _context = context;
            _serviceScopeFactory = serviceScopeFactory;
            _configuration = configuration;
        }

        public async Task CreateAppDraftTemplates(AppDraft appDraft)
        {
            using (var _scope = _serviceScopeFactory.CreateScope())
            {
                var databaseContext = _scope.ServiceProvider.GetRequiredService<StudioDBContext>();
                using (var _appDraftTemplateRepository = new AppDraftTemplateRepository(databaseContext, _configuration))
                {
                    //Default templates
                    _appDraftTemplateRepository.CurrentUser = new CurrentUser{PreviewMode = "app", TenantId = appDraft.Id, UserId = 1};

                    var appDraftTemplates = await _appDraftTemplateRepository.GetAllById(1);
                    foreach (var appDraftTemplate in appDraftTemplates)
                    {
                        var newAppDraftTemplate = new AppDraftTemplate
                        {
                            AppId = appDraft.Id,
                            Name = appDraftTemplate.Name,
                            Subject = appDraftTemplate.Subject,
                            Content = appDraftTemplate.Content,
                            Language = appDraftTemplate.Language,
                            Type = appDraftTemplate.Type,
                            SystemCode = appDraftTemplate.SystemCode,
                            Active = appDraftTemplate.Active,
                            Settings = appDraftTemplate.Settings,
                            CreatedById = 1
                        };

                        await _appDraftTemplateRepository.Create(newAppDraftTemplate);
                    }
                }
            }
        }
    }
}