using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class ModuleProfileSettingRepository : RepositoryBaseTenant, IModuleProfileSettingRepository
    {
        public ModuleProfileSettingRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<ICollection<ModuleProfileSetting>> GetAllBasic()
        {
            var moduleProfileSettings = await DbContext.ModuleProfileSettings
                .Where(x => !x.Deleted)
                .ToListAsync();

            return moduleProfileSettings;
        }

        public async Task<ModuleProfileSetting> GetByIdBasic(int id)
        {
            var moduleProfileSetting = await DbContext.ModuleProfileSettings
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return moduleProfileSetting;
        }

        public async Task<int> Create(ModuleProfileSetting moduleProfileSetting)
        {
            DbContext.ModuleProfileSettings.Add(moduleProfileSetting);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(ModuleProfileSetting moduleProfileSetting)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(ModuleProfileSetting moduleProfileSetting)
        {
            moduleProfileSetting.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Count(int id)
        {
            var count = DbContext.ModuleProfileSettings
               .Where(x => !x.Deleted && x.ModuleId == id).Count();
            return count;
        }

        public async Task<ICollection<ModuleProfileSetting>> Find(PaginationModel paginationModel)
        {
            var moduleProfilesSettings = GetPaginationGQuery(paginationModel)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit).ToList();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(ModuleProfileSetting).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    moduleProfilesSettings = moduleProfilesSettings.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    moduleProfilesSettings = moduleProfilesSettings.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }

            }
            
            return moduleProfilesSettings;

        }

        private IQueryable<ModuleProfileSetting> GetPaginationGQuery(PaginationModel paginationModel, bool withIncludes = true)
        {
            return DbContext.ModuleProfileSettings
                 .Where(x => !x.Deleted).OrderByDescending(x => x.Id);
        }
    }
}
