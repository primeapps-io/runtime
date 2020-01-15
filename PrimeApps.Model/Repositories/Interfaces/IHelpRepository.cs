using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IHelpRepository : IRepositoryBaseTenant
    {
        Task<Help> GetById(int id);
        Task<Help> GetByIdBasic(int id);
        Task<int> Create(Help help);
        Task<int> Update(Help help);
        Task<int> DeleteSoft(Help help);
        Task<int> DeleteHard(Help help);
        Task<ICollection<Help>> GetAll(ModalType helpType = ModalType.NotSet, LanguageType language = LanguageType.NotSet);
        Task<Help> GetByType(ModalType helpType, LanguageType language = LanguageType.NotSet, int? moduleId = null, string route = "");
        Task<Help> GetFistScreen(ModalType helpType, LanguageType language = LanguageType.NotSet, bool? firstscreen = false);
        Task<ICollection<Help>> GetCustomHelp(ModalType helpType, LanguageType language = LanguageType.NotSet, bool? customhelp = false);
        Task<Help> GetModuleType(ModalType helpType, ModuleType moduleType, LanguageType language = LanguageType.NotSet, int? moduleId = null);
        Task<int> Count();
        IQueryable<Help> Find();

    }
}
