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
    public class HelpRepository : RepositoryBaseTenant, IHelpRepository
    {
        public HelpRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<Help> GetById(int id)
        {
            var help = await DbContext.Helps
                .Where(x => !x.Deleted && x.Id == id)
                .FirstOrDefaultAsync();

            return help;
        }

        public async Task<int> Count()
        {
            var count = await DbContext.Helps
                .Where(x => !x.Deleted).CountAsync();

            return count;
        }

        public IQueryable<Help> Find()
        {
            var helps = DbContext.Helps
            .Where(x => !x.Deleted)
            .Include(x=>x.Module)
            .OrderByDescending(x => x.Id);

            return helps;
        }

        public async Task<Help> GetByIdBasic(int id)
        {
            var help = await DbContext.Helps
                .Where(x => !x.Deleted && x.Id == id)
                .FirstOrDefaultAsync();

            return help;
        }

        public async Task<int> Create(Help help)
        {
            DbContext.Helps.Add(help);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Help help)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(Help help)
        {
            help.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Help help)
        {
            DbContext.Helps.Remove(help);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<ICollection<Help>> GetAll(ModalType modalType = ModalType.NotSet, LanguageType language = LanguageType.NotSet)
        {
            var helps = DbContext.Helps
                .Where(x => !x.Deleted);

            if (modalType != ModalType.NotSet)
                helps = helps.Where(x => x.ModalType == modalType);

            if (language != LanguageType.NotSet)
                helps = helps.Where(x => x.Language == language);

            helps = helps.OrderBy(x => x.CreatedAt);

            return await helps.ToListAsync();
        }

        public async Task<Help> GetByType(ModalType templateType, LanguageType language = LanguageType.NotSet, int? moduleId = null, string route = "")
        {
            var helps = DbContext.Helps
                .Where(x => x.Deleted == false)
                .Where(x => x.ModalType == templateType);

            if (language != LanguageType.NotSet)
                helps = helps.Where(x => x.Language == language);
            
            if (moduleId.HasValue)
                helps = helps.Where(x => x.ModuleId == moduleId);

            if (!string.IsNullOrEmpty(route) && route != "null")
                helps = helps.Where(x => x.RouteUrl == route);

            helps = helps.OrderByDescending(x => x.CreatedAt);

            return await helps.FirstOrDefaultAsync();
        }

        public async Task<Help> GetModuleType(ModalType templateType, ModuleType moduleType, LanguageType language = LanguageType.NotSet, int? moduleId = null)
        {
            var helps = DbContext.Helps
                .Where(x => x.Deleted == false)
                .Where(x => x.ModalType == templateType)
                .Where(x => x.ModuleType == moduleType);

            if (language != LanguageType.NotSet)
                helps = helps.Where(x => x.Language == language);
            
            if (moduleId.HasValue)
                helps = helps.Where(x => x.ModuleId == moduleId);
            
            helps = helps.OrderByDescending(x => x.CreatedAt);

            return await helps.FirstOrDefaultAsync();
        }

        public async Task<Help> GetFistScreen(ModalType templateType, LanguageType language = LanguageType.NotSet, bool? firstscreen = false)
        {
            var helps = DbContext.Helps
                .Where(x => x.Deleted == false)
                .Where(x => x.ModalType == templateType);

            if (language != LanguageType.NotSet)
                helps = helps.Where(x => x.Language == language);
            
            if (firstscreen != false)
                helps = helps.Where(x => x.FirstScreen == firstscreen);

            helps = helps.OrderByDescending(x => x.CreatedAt);

            return await helps.FirstOrDefaultAsync();
        }

        public async Task<ICollection<Help>> GetCustomHelp(ModalType templateType, LanguageType language = LanguageType.NotSet, bool? customhelp = false)
        {
            var helps = DbContext.Helps
                .Where(x => x.Deleted == false)
                .Where(x => x.ModalType == templateType);

            if (language != LanguageType.NotSet)
                helps = helps.Where(x => x.Language == language);
            
            if (customhelp != false)
                helps = helps.Where(x => x.CustomHelp == customhelp);

            helps = helps.OrderByDescending(x => x.CreatedAt);

            return await helps.ToListAsync();
        }
    }
}