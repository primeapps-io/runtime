using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Hangfire;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common.Import;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class ImportRepository : RepositoryBaseTenant, IImportRepository
    {
        private Warehouse _warehouse;

        public ImportRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public ImportRepository(TenantDBContext dbContext, Warehouse warehouse, IConfiguration configuration) : base(dbContext, configuration)
        {
            _warehouse = warehouse;
        }

        public async Task<Import> GetById(int id)
        {
            var import = await DbContext.Imports
                .Include(x => x.Module)
                .Include(x => x.CreatedBy)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return import;
        }

        public async Task<ICollection<Import>> Find(ImportRequest request)
        {
            var imports = GetImportQuery(request);

            imports = imports
                .OrderByDescending(x => x.CreatedAt)
                .Skip(request.Offset)
                .Take(request.Limit);

            return await imports.ToListAsync();
        }

        public async Task<int> Count(ImportRequest request)
        {
            var totalCount = await GetImportQuery(request, false).CountAsync();

            return totalCount;
        }

        public async Task<int> Create(Import import)
        {
            DbContext.Imports.Add(import);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Import import)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(Import import)
        {
            import.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(Import import)
        {
            DbContext.Imports.Remove(import);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Revert(Import import)
        {
            var sql = RecordHelper.GenerateRevertSql(import.Module.Name, import.Id);
            var result = await DbContext.Database.ExecuteSqlCommandAsync(sql);

            if (result > 0)
            {
                // Create warehouse record
                if (string.IsNullOrWhiteSpace(_warehouse?.DatabaseName))
                    throw new Exception("Warehouse cannot be null during create/update/delete record.");

                if (_warehouse.DatabaseName != "0")
                    BackgroundJob.Enqueue(() => _warehouse.ImportRevert(import.Id, _warehouse.DatabaseName, import.Module.Name, CurrentUser));
            }

            return result;
        }

        private IQueryable<Import> GetImportQuery(ImportRequest request, bool withIncludes = true)
        {
            var imports = DbContext.Imports
                .Where(x => !x.Deleted);

            if (withIncludes)
            {
                imports = imports
                    .Include(x => x.Module)
                    .Include(x => x.CreatedBy);
            }

            if (request.ModuleId.HasValue)
                imports = imports.Where(x => x.ModuleId == request.ModuleId.Value);

            if (request.UserId.HasValue)
                imports = imports.Where(x => x.CreatedById == request.UserId.Value);

            return imports;
        }
    }
}

