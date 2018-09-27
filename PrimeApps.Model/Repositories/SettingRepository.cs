using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Model.Repositories
{
    public class SettingRepository : RepositoryBaseTenant, ISettingRepository
    {
        public SettingRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<Setting> GetById(int id)
        {
            var setting = await DbContext.Settings
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return setting;
        }

        public async Task<IList<Setting>> GetAllSettings()
        {
            return await DbContext.Settings.Where(r => r.Deleted == false).ToListAsync();
        }

        public async Task<IList<Setting>> GetAllSettings(int userId)
        {
            return await DbContext.Settings.Where(r => r.Deleted == false && (r.UserId == userId || r.UserId == null)).ToListAsync();
        }

        public IList<Setting> Get(SettingType settingType)
        {
            return DbContext.Settings.Where(r => r.Type == settingType && r.Deleted == false).ToList();
        }

        public async Task<IList<Setting>> GetAsync(SettingType settingType, int userId)
        {
            return await DbContext.Settings.Where(r => r.Type == settingType && r.UserId == userId && r.Deleted == false).ToListAsync();
        }

        public async Task<IList<Setting>> GetAsync(SettingType settingType)
        {
            return await DbContext.Settings.Where(r => r.Type == settingType && r.Deleted == false).ToListAsync();
        }

        public async Task<IList<Setting>> GetByValueAsync(SettingType settingType, string value)
        {
            return await DbContext.Settings.Where(r => r.Value == value && r.Type == settingType && r.Deleted == false).ToListAsync();
        }

        public async Task<Setting> GetByKeyAsync(string key, int? userId = 0)
        {
            if (userId.HasValue && userId > 0)
                return await DbContext.Settings.FirstOrDefaultAsync(r => r.Key == key && r.UserId == userId && r.Deleted == false);

            return await DbContext.Settings.FirstOrDefaultAsync(r => r.Key == key && r.Deleted == false);
        }

        public async Task<int> Create(Setting setting)
        {
            DbContext.Settings.Add(setting);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Setting setting)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(Setting setting)
        {
            setting.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Setting setting)
        {
            DbContext.Settings.Remove(setting);

            return await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes tenant based settings where it doesn't belong to an user.
        /// </summary>
        /// <param name="settingType"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(SettingType settingType)
        {
            var settings = await DbContext.Settings.Where(r => r.Type == settingType && r.UserId == null).ToListAsync();
            DbContext.Settings.RemoveRange(settings);
            var count = await DbContext.SaveChangesAsync();
            return count > 0 ? true : false;
        }

        /// <summary>
        /// Deletes settings related to a single user.
        /// </summary>
        /// <param name="settingType"></param>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(SettingType settingType, int userId)
        {
            var settings = await DbContext.Settings.Where(r => r.Type == settingType && r.UserId == userId).ToListAsync();
            DbContext.Settings.RemoveRange(settings);
            var count = await DbContext.SaveChangesAsync();
            return count > 0 ? true : false;
        }

        /// <summary>
        /// Deletes single setting related with key
        /// </summary>
        /// <param name="settingType"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public async Task<bool> DeleteAsync(SettingType settingType, string key)
        {
            var setting = await DbContext.Settings.Where(r => r.Type == settingType && r.Key == key).FirstOrDefaultAsync();
            var count = 0;
            if (setting != null)
            {
                DbContext.Settings.Remove(setting);
                count = await DbContext.SaveChangesAsync();
            }

            return count > 0 ? true : false;
        }

        public async Task<int> AddSettings(IList<Setting> settings)
        {
            foreach (var setting in settings)
            {
                DbContext.Settings.Add(setting);
            }
            return await DbContext.SaveChangesAsync();
        }
    }
}
