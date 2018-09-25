using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ISettingRepository : IRepositoryBaseTenant
    {
        Task<Setting> GetById(int id);
        Task<IList<Setting>> GetAllSettings();
        Task<IList<Setting>> GetAllSettings(int userId);
        IList<Setting> Get(SettingType settingType);
        Task<IList<Setting>> GetAsync(SettingType settingType, int userId);
        Task<IList<Setting>> GetAsync(SettingType settingType);
        Task<IList<Setting>> GetByValueAsync(SettingType settingType, string value);
        Task<Setting> GetByKeyAsync(string key, int? userId = 0);
        Task<int> Create(Setting setting);
        Task<int> Update(Setting setting);
        Task<int> DeleteSoft(Setting setting);
        Task<int> DeleteHard(Setting setting);
        Task<bool> DeleteAsync(SettingType settingType);
        Task<bool> DeleteAsync(SettingType settingType, int userId);
        Task<bool> DeleteAsync(SettingType settingType, string key);
        Task<int> AddSettings(IList<Setting> settings);
    }
}
