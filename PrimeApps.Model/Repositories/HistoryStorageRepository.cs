using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.SqlServer.Management.Smo;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class HistoryStorageRepository : RepositoryBaseTenant, IHistoryStorageRepository
    {
        public HistoryStorageRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<List<HistoryStorage>> GetDiffs(string min)
        {
            var current = await DbContext.HistoryStorages.FirstOrDefaultAsync(x => x.Tag == min);
            if (current != null)
                return await DbContext.HistoryStorages.Where(x => x.Id > current.Id).ToListAsync();

            current = await DbContext.HistoryStorages.FirstOrDefaultAsync(x => x.Tag == (int.Parse(min) + 1).ToString());

            if (current == null)
                return null;

            return await DbContext.HistoryStorages.Where(x => !x.Deleted).ToListAsync();
        }

        public async Task<HistoryStorage> GetLast()
        {
            return await DbContext.HistoryStorages.OrderByDescending(x => x.Id).FirstOrDefaultAsync();
        }

        public async Task<int> Create(HistoryStorage historyStorage)
        {
            DbContext.HistoryStorages.Add(historyStorage);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(HistoryStorage historyDatabase)
        {
            return await DbContext.SaveChangesAsync();
        }
    }
}