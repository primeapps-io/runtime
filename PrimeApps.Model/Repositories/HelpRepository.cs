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
        public HelpRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<Help> GetById(int id)
        {
            var help = await DbContext.Helps
                .Where(x => !x.Deleted && x.Id == id)
                .FirstOrDefaultAsync();

            return help;
        }

        public async Task<int> Count()
        {
            var count = DbContext.Helps
               .Where(x => !x.Deleted).Count();
            return count;
        }

        public async Task<ICollection<Help>> Find(PaginationModel paginationModel)
        {
            var helps = GetPaginationGQuery(paginationModel)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit).ToList();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Help).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    helps = helps.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    helps = helps.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }

            }

            return helps;

        }

        private IQueryable<Help> GetPaginationGQuery(PaginationModel paginationModel, bool withIncludes = true)
        {
            return DbContext.Helps
                 .Where(x => !x.Deleted).OrderByDescending(x => x.Id);

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

        public async Task<ICollection<Help>> GetAll(ModalType modalType = ModalType.NotSet)
        {
            var helps = DbContext.Helps
                .Where(x => !x.Deleted);

            if (modalType != ModalType.NotSet)
                helps = helps.Where(x => x.ModalType == modalType);

            helps = helps.OrderBy(x => x.CreatedAt);

            return await helps.ToListAsync();
        }

        public async Task<Help> GetByType(ModalType templateType, int? moduleId = null, string route = "")
        {
            var templates = DbContext.Helps
                .Where(x => x.Deleted == false)
                .Where(x => x.ModalType == templateType);


            if (moduleId.HasValue)
                templates = templates.Where(x => x.ModuleId == moduleId);


            if (!string.IsNullOrEmpty(route) && route != "null")
                templates = templates.Where(x => x.RouteUrl == route);

            templates = templates.OrderByDescending(x => x.CreatedAt);

            return await templates.FirstOrDefaultAsync();
        }

        public async Task<Help> GetModuleType(ModalType templateType, ModuleType moduleType, int? moduleId = null)
        {
            var templates = DbContext.Helps
                .Where(x => x.Deleted == false)
                .Where(x => x.ModalType == templateType)
                .Where(x => x.ModuleType == moduleType);


            if (moduleId.HasValue)
                templates = templates.Where(x => x.ModuleId == moduleId);


            templates = templates.OrderByDescending(x => x.CreatedAt);

            return await templates.FirstOrDefaultAsync();
        }

        public async Task<Help> GetFistScreen(ModalType templateType, bool? firstscreen = false)
        {
            var templates = DbContext.Helps
                .Where(x => x.Deleted == false)
                .Where(x => x.ModalType == templateType);

            if (firstscreen != false)
                templates = templates.Where(x => x.FirstScreen == firstscreen);

            templates = templates.OrderByDescending(x => x.CreatedAt);

            return await templates.FirstOrDefaultAsync();
        }

        public async Task<ICollection<Help>> GetCustomHelp(ModalType templateType, bool? customhelp = false)
        {
            var templates =  DbContext.Helps
                .Where(x => x.Deleted == false)
                .Where(x => x.ModalType == templateType);

            if (customhelp != false)
                templates = templates.Where(x => x.CustomHelp == customhelp);

            templates = templates.OrderByDescending(x => x.CreatedAt);

            return await  templates.ToListAsync();
        }
    }
}

