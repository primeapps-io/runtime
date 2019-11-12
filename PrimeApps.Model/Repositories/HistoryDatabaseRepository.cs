using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class HistoryDatabaseRepository : RepositoryBaseTenant, IHistoryDatabaseRepository
    {
        public HistoryDatabaseRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<List<HistoryDatabase>> GetDiffs(string min)
        {
            var current = await DbContext.HistoryDatabases.FirstOrDefaultAsync(x => x.Tag == min);
            if (current != null)
                return await DbContext.HistoryDatabases.Where(x => x.Id > current.Id).OrderBy(x => x.Id).ToListAsync();

            current = await DbContext.HistoryDatabases.FirstOrDefaultAsync(x => x.Tag == (int.Parse(min) + 1).ToString());

            if (current == null)
                return null;

            return await DbContext.HistoryDatabases.Where(x => !x.Deleted).OrderBy(x => x.Id).ToListAsync();
        }

        public async Task<HistoryDatabase> Get(Guid commandId)
        {
            return await DbContext.HistoryDatabases.FirstOrDefaultAsync(x => x.CommandId == commandId);
        }

        public async Task<HistoryDatabase> Get(string tag)
        {
            return await DbContext.HistoryDatabases.FirstOrDefaultAsync(x => x.Tag == tag);
        }

        public async Task<HistoryDatabase> GetLast()
        {
            return await DbContext.HistoryDatabases.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        }

        public async Task<int> Create(HistoryDatabase historyDatabase)
        {
            DbContext.HistoryDatabases.Add(historyDatabase);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(HistoryDatabase historyDatabase)
        {
            return await DbContext.SaveChangesAsync();
        }
    }
}