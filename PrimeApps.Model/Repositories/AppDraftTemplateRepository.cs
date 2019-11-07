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

        public async Task<ICollection<AppDraftTemplate>> Find(PaginationModel paginationModel, int? appId)
        {
            var templates = DbContext.AppTemplates
                .Where(x => !x.Deleted && x.Type == AppTemplateType.Email && x.AppId == appId)
                .OrderByDescending(x => x.Id) //&& x.Active
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(AppDraftTemplate).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

                if (paginationModel.OrderType == "asc")
                {
                    templates = templates.OrderBy(x => propertyInfo.GetValue(x, null));
                }
                else
                {
                    templates = templates.OrderByDescending(x => propertyInfo.GetValue(x, null));
                }
            }

            return await templates.ToListAsync();
        }

        public int Count(int appId)
        {
            return DbContext.AppTemplates.Count(x => !x.Deleted && x.Type == AppTemplateType.Email && x.AppId == appId);
        }

        public async Task<AppDraftTemplate> Get(int id)
        {
            return await DbContext.AppTemplates.FirstOrDefaultAsync(x => x.Id == id);
        }
    }
}