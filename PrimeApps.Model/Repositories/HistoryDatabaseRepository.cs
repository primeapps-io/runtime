using System;
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
        
        public async Task<HistoryDatabase> Get(Guid commandId)
        {
            return await DbContext.HistoryDatabases.FirstOrDefaultAsync(x => x.CommandId == commandId);
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