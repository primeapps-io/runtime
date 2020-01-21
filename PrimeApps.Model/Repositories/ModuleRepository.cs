using System;
using System.Collections.Generic;
using System.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories
{
    public class ModuleRepository : RepositoryBaseTenant, IModuleRepository
    {
        private Warehouse _warehouse;

        public ModuleRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext,
            configuration)
        {
        }

        public ModuleRepository(TenantDBContext dbContext, Warehouse warehouse, IConfiguration configuration) : base(
            dbContext, configuration)
        {
            _warehouse = warehouse;
        }

        public async Task<int> Count()
        {
            var count = await DbContext.Modules
                .Where(x => !x.Deleted && x.Order != 0).CountAsync();
            return count;
        }

        //public async Task<ICollection<Module>> Find(PaginationModel paginationModel)
        //{
        //    var modules = DbContext.Modules
        //        .Where(x => !x.Deleted && x.Order != 0)
        //        .OrderByDescending(x => x.Id)
        //        .Skip(paginationModel.Offset * paginationModel.Limit)
        //        .Take(paginationModel.Limit);

        //    if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
        //    {
        //        var propertyInfo = typeof(Module).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

        //        if (paginationModel.OrderType == "asc")
        //        {
        //            modules = modules.OrderBy(x => propertyInfo.GetValue(x, null));
        //        }
        //        else
        //        {
        //            modules = modules.OrderByDescending(x => propertyInfo.GetValue(x, null));
        //        }
        //    }

        //    return await modules.ToListAsync();
        //}

        public IQueryable<Module> Find()
        {
            var modules = DbContext.Modules
            .Where(x => !x.Deleted && x.Order != 0)
            .OrderByDescending(x => x.Id);

            return modules;
        }

        public async Task<ICollection<Module>> GetAllBasic()
        {
            var modules = await DbContext.Modules.Where(x => !x.Deleted)
                .Select(x => new Module
                {
                    Id = x.Id,
                    Name = x.Name,
                    LabelEnPlural = x.LabelEnPlural,
                    LabelEnSingular = x.LabelEnSingular,
                    LabelTrSingular = x.LabelTrSingular,
                    LabelTrPlural = x.LabelTrPlural,
                    Order = x.Order,
                    SystemType = x.SystemType,
                    Display = x.Display,
                    MenuIcon = x.MenuIcon
                })
                .ToListAsync();

            return modules;
        }

        public async Task<Module> GetById(int id)
        {
            var module = await GetModuleQuery()
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

            return module;
        }

        public async Task<Module> GetByLabel(string name)
        {
            var module = await GetModuleQuery()
                .FirstOrDefaultAsync(
                    x =>
                        x.LabelTrPlural == name || x.LabelEnPlural == name ||
                        x.LabelEnSingular == name || x.LabelTrSingular == name
                        && !x.Deleted);

            return module;
        }

        public async Task<Module> GetByName(string name)
        {
            var module = await GetModuleQuery()
                .FirstOrDefaultAsync(x => x.Name == name && !x.Deleted);

            return module;
        }

        public async Task<Module> GetBasicByName(string name)
        {
            var module = await DbContext.Modules
                .Include(x => x.Fields)
                .FirstOrDefaultAsync(x => x.Name == name && !x.Deleted);

            return module;
        }

        public async Task<Module> GetByIdBasic(int id)
        {
            var module = await DbContext.Modules
                .Include(x => x.Fields)
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

            return module;
        }

        public async Task<Module> GetByNameBasic(string name)
        {
            var module = await DbContext.Modules
                .Include(x => x.Fields)
                .ThenInclude(field => field.Permissions)
                .FirstOrDefaultAsync(x => x.Name == name && !x.Deleted);

            return module;
        }

        public async Task<ICollection<Module>> GetByNamesBasic(List<string> names)
        {
            var modules = await DbContext.Modules
                .Include(x => x.Fields)
                .Where(x => names.Contains(x.Name) && !x.Deleted)
                .ToListAsync();

            return modules;
        }

        public async Task<Module> GetByNameWithDependencies(string name)
        {
            var module = await DbContext.Modules
                .Include(x => x.Fields)
                .ThenInclude(field => field.Permissions)
                .Include(x => x.Fields)
                .ThenInclude(field => field.Combination)
                .Include(x => x.Dependencies)
                .FirstOrDefaultAsync(x => x.Name == name && !x.Deleted);

            return module;
        }

        public async Task<ICollection<Module>> GetAll()
        {
            var modules = await GetModuleFullQuery()
                .Where(x => !x.Deleted)
                .Include(x => x.Fields)
                .ToListAsync();

            return modules;
        }

        public async Task<ICollection<Component>> GetComponents()
        {
            var components = await DbContext.Modules
                .Include(x => x.Components)
                .Where(x => x.SystemType == SystemType.Component)
                .ToListAsync();

            return components.SelectMany(x => x.Components).Where(x => x.Type == ComponentType.Component).ToList();
        }

        public async Task<ICollection<Module>> GetAllDeleted()
        {
            var modules = await GetModuleQuery()
                .Where(x => x.Deleted)
                .ToListAsync();

            return modules;
        }

        public async Task<int> Create(Module module)
        {
            if (module.Order == 0)
            {
                var order = DbContext.Modules.Count();
                module.Order = (short)(order + 1);
            }

            DbContext.Modules.Add(module);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> CreateTable(Module module, string language)
        {
            var tableCreateSql = ModuleHelper.GenerateTableCreateSql(module, language);

            var result = await DbContext.Database.ExecuteSqlCommandAsync(tableCreateSql);

            //if (result == -1)
            //{
            //    // Create warehouse table
            //    if (string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
            //        throw new Exception("Warehouse cannot be null during create/update module.");

            //    if (_warehouse.DatabaseName != "0")
            //        BackgroundJob.Enqueue(() => _warehouse.CreateTable(_warehouse.DatabaseName, module.Name, CurrentUser, language));
            //}

            return result;
        }

        public async Task<int> CreateIndexes(Module module)
        {
            var indexesCreateSql = ModuleHelper.GenerateIndexesCreateSql(module);

            return await DbContext.Database.ExecuteSqlCommandAsync(indexesCreateSql);
        }

        public async Task<int> Update(Module module)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateField(Field Field)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> AlterTable(Module module, ModuleChanges moduleChanges, string language)
        {
            var tableAlterSql = ModuleHelper.GenerateTableAlterSql(module, moduleChanges, language);

            if (string.IsNullOrEmpty(tableAlterSql))
                return -1;

            var result = await DbContext.Database.ExecuteSqlCommandAsync(tableAlterSql);


            return result;
        }

        public async Task<int> DeleteSoft(Module module)
        {
            module.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Module module)
        {
            DbContext.Modules.Remove(module);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteTable(Module module)
        {
            var tableDropSql = ModuleHelper.GenerateTableDropSql(module);

            return await DbContext.Database.ExecuteSqlCommandAsync(tableDropSql);
        }

        public async Task<Relation> GetRelation(int id)
        {
            var relation = await DbContext.Relations
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

            return relation;
        }

        public async Task<int> CreateRelation(Relation relation)
        {
            DbContext.Relations.Add(relation);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> CreateJunctionTable(Module module, Relation relation,
            ICollection<Relation> currentRelations)
        {
            var tableJunctionCreateSql =
                ModuleHelper.GenerateJunctionTableCreateSql(module.Name, relation.RelatedModule, currentRelations);

            var result = await DbContext.Database.ExecuteSqlCommandAsync(tableJunctionCreateSql);

            return result;
        }

        public async Task<int> UpdateRelation(Relation relation)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteRelationSoft(Relation relation)
        {
            relation.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteRelationHard(Relation relation)
        {
            DbContext.Relations.Remove(relation);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<Dependency> GetDependency(int id)
        {
            var dependency = await DbContext.Dependencies
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

            return dependency;
        }

        public async Task<Field> GetField(int id)
        {
            var field = await DbContext.Fields
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

            return field;
        }

        public async Task<int> CreateDependency(Dependency dependency)
        {
            DbContext.Dependencies.Add(dependency);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> UpdateDependency(Dependency dependency)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteDependencySoft(Dependency dependency)
        {
            dependency.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteDependencyHard(Dependency dependency)
        {
            DbContext.Dependencies.Remove(dependency);

            return await DbContext.SaveChangesAsync();
        }

        /// <summary>
        /// Gets modules created by the user.
        /// </summary>
        /// <param name="userId"></param>
        /// <returns></returns>
        public async Task<ICollection<Module>> GetByUserID(int userId)
        {
            var module = await DbContext.Modules
                .Where(x => x.CreatedById == userId && !x.Deleted)
                .ToListAsync();

            return module;
        }

        private IQueryable<Module> GetModuleQuery()
        {
            return DbContext.Modules
                .Include(module => module.Sections)
                .ThenInclude(section => section.Permissions)
                .Include(module => module.Fields)
                .ThenInclude(field => field.Validation)
                .Include(module => module.Fields)
                .ThenInclude(field => field.Combination)
                .Include(module => module.Fields)
                .ThenInclude(field => field.Filters)
                .Include(module => module.Fields)
                .ThenInclude(field => field.Permissions);
        }

        public async Task<Field> GetFieldByName(string fieldName)
        {
            var field = await DbContext.Fields
                .FirstOrDefaultAsync(x => x.Name == fieldName && !x.Deleted);
            return field;
        }

        public async Task<ICollection<Field>> GetModuleFieldByName(string moduleName)
        {
            var module = await GetModuleQuery()
                .FirstOrDefaultAsync(x => x.Name == moduleName && !x.Deleted);

            return module.Fields;
        }

        public async Task<Module> GetByIdFullModule(int id)
        {
            var module = await GetModuleFullQuery()
                .FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

            return module;
        }

        public async Task<Module> GetByNameFullModule(string name)
        {
            return await GetModuleFullQuery()
                .FirstOrDefaultAsync(x => x.Name == name && !x.Deleted);
        }

        private IQueryable<Module> GetModuleFullQuery()
        {
            return DbContext.Modules
                .Include(module => module.Sections)
                .ThenInclude(section => section.Permissions)
                .Include(module => module.Fields)
                .ThenInclude(field => field.Validation)
                .Include(module => module.Fields)
                .ThenInclude(field => field.Combination)
                .Include(module => module.Fields)
                .ThenInclude(field => field.Filters)
                .Include(module => module.Fields)
                .ThenInclude(field => field.Permissions)
                .Include(module => module.Relations)
                .Include(module => module.Dependencies)
                .Include(module => module.Calculations)
                .Include(module => module.Components);
        }
    }
}