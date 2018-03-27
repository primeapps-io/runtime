using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class TemplateRepository : RepositoryBaseTenant, ITemplateRepostory
    {
        public TemplateRepository(TenantDBContext dbContext) : base(dbContext) { }

        public async Task<Template> GetById(int id)
        {

            var template = await DbContext.Templates.Include(x=> x.Permissions).FirstOrDefaultAsync(x => x.Id == id);

            return template;
        }

        public async Task<ICollection<Template>> GetAll(TemplateType templateType = TemplateType.NotSet, string moduleName = "")
        {

            var templates = DbContext.Templates
                .Include(x => x.Shares)
                .Include(x => x.Permissions)
                .Where(x => x.Deleted == false);

            if (templateType != TemplateType.NotSet)
                templates = templates.Where(x => x.TemplateType == templateType);

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
    }
}

