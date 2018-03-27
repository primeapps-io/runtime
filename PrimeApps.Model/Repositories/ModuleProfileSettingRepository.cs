using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class ModuleProfileSettingRepository : RepositoryBaseTenant, IModuleProfileSettingRepository
    {
        public ModuleProfileSettingRepository(TenantDBContext dbContext) : base(dbContext) { }

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
    }
}
