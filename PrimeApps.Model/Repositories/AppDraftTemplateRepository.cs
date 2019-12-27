using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class AppDraftTemplateRepository : RepositoryBaseStudio, IAppDraftTemplateRepository
    {
        public AppDraftTemplateRepository(StudioDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration)
        {
        }

        public async Task<List<AppDraftTemplate>> GetAll(int appId)
        {
            return await DbContext.AppTemplates.Where(x => x.AppId == appId).ToListAsync();
        }

        public async Task<int> Create(AppDraftTemplate template)
        {
            DbContext.AppTemplates.Add(template);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(AppDraftTemplate template)
        {
            return await DbContext.SaveChangesAsync();
        }

        public IQueryable<AppDraftTemplate> Find()
        {
            var templates = DbContext.AppTemplates
            .Where(x => !x.Deleted)
            .OrderByDescending(x => x.Id);

            return templates;
        }

        public int Count(int appId)
        {
            return DbContext.AppTemplates.Count(x => !x.Deleted && x.Type == AppTemplateType.Email && x.AppId == appId);
        }

        public async Task<AppDraftTemplate> Get(int id)
        {
            return await DbContext.AppTemplates.FirstOrDefaultAsync(x => x.Id == id);
        }
        
        public async Task<int> DeleteSoft(AppDraftTemplate template)
        {
            template.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }
    }
}