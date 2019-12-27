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
using System.Linq.Dynamic.Core;

namespace PrimeApps.Model.Repositories
{
    public class ModuleProfileSettingRepository : RepositoryBaseTenant, IModuleProfileSettingRepository
    {
        public ModuleProfileSettingRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

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
            var count = await DbContext.ModuleProfileSettings
                .Where(x => !x.Deleted && x.ModuleId == id).CountAsync();

            return count;
        }

        public IQueryable<ModuleProfileSetting> Find()
        {
            var moduleProfilesSettings = DbContext.ModuleProfileSettings
                .Where(x => !x.Deleted)
                .OrderByDescending(x => x.Id);

            return moduleProfilesSettings;
        }

        public async Task<List<string>> GetUseProfileIdsByModuleId(int moduleId)
        {
            var list = await DbContext.ModuleProfileSettings
            .Where(x => !x.Deleted && x.ModuleId == moduleId)
            .Select(y => y.Profiles).ToListAsync();

            return list;
        }
    }
}