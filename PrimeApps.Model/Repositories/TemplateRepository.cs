using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class TemplateRepository : RepositoryBaseTenant, ITemplateRepository
    {
        public TemplateRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<Template> GetById(int id)
        {
            var template = await DbContext.Templates.Include(x => x.Permissions).FirstOrDefaultAsync(x => x.Id == id);
            template.Permissions = template.Permissions.Where(q => !q.Deleted).ToList();

            return template;
        }

        public Template GetByCode(string code, LanguageType language = LanguageType.Tr)
        {
            return DbContext.Templates.FirstOrDefault(x => x.Code == code && x.Language == language && !x.Deleted);
        }

        public async Task<ICollection<Template>> GetByType(TemplateType templateType)
        {
            return await DbContext.Templates
                .Where(x => x.TemplateType == templateType && x.Deleted == false)
                .ToListAsync();
        }
        
        public async Task<ICollection<Template>> GetAll(TemplateType templateType, LanguageType language, bool hasNotCode = true, string moduleName = "")
        {
            var templates = DbContext.Templates
                .Include(x => x.Shares)
                .ThenInclude(x => x.TenantUser)
                .Include(x => x.Permissions)
                .Where(x => x.Deleted == false);

            if (language != LanguageType.NotSet)
                templates = templates.Where(x => x.Language == language);

            if (hasNotCode)
                templates = templates.Where(x => x.Code == null);

            if (templateType != TemplateType.NotSet)
                templates = templates.Where(x => x.TemplateType == templateType);

            if (!string.IsNullOrEmpty(moduleName))
                templates = templates.Where(x => x.Module == moduleName || string.IsNullOrEmpty(x.Module));

            if (templateType == TemplateType.Email)
            {
                templates = templates.Where(x => x.SharingType == TemplateSharingType.Everybody
                                                 || x.CreatedBy.Id == CurrentUser.UserId
                                                 || x.Shares.Any(j => j.UserId == CurrentUser.UserId));

                return await templates.ToListAsync();
            }
            else
            {
                templates = templates.OrderByDescending(x => x.CreatedAt);

                return await templates.ToListAsync();
            }
        }

        public async Task<ICollection<Template>> GetAllList(TemplateType templateType = TemplateType.NotSet, TemplateType excelType = TemplateType.NotSet, string moduleName = "")
        {
            var templates = DbContext.Templates
                .Include(x => x.Shares)
                .Include(x => x.Permissions)
                .Where(x => x.Deleted == false);

            if (templateType != TemplateType.NotSet && excelType != TemplateType.NotSet)
                templates = templates.Where(x => x.TemplateType == templateType || x.TemplateType == excelType);

            if (!string.IsNullOrEmpty(moduleName))
                templates = templates.Where(x => x.Module == moduleName || string.IsNullOrEmpty(x.Module));

            templates = templates.OrderByDescending(x => x.CreatedAt);

            return await templates.ToListAsync();
        }

        public async Task<int> Create(Template template)
        {
            DbContext.Templates.Add(template);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> CreateExcel(Template template)
        {
            DbContext.Templates.Add(template);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Template template)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(Template template)
        {
            template.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Template template)
        {
            DbContext.Templates.Remove(template);

            return await DbContext.SaveChangesAsync();
        }

        public int Count(TemplateType templateType)
        {
            var count = DbContext.Templates
                .Where(x => !x.Deleted && x.TemplateType == templateType).Count();
            return count;
        }

        public async Task<ICollection<Template>> Find(PaginationModel paginationModel, TemplateType templateType)
        {
            var templates = DbContext.Templates
                .Where(x => !x.Deleted && x.TemplateType == templateType)
                .OrderByDescending(x => x.Id)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit);

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Template).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

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
    }
}