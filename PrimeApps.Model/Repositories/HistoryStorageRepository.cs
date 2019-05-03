using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class HistoryStorageRepository: RepositoryBaseTenant, IHistoryStorageRepository
    {
        public HistoryStorageRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }
        
        public async Task<int> Create(HistoryStorage historyStorage)
        {
            DbContext.HistoryStorages.Add(historyStorage);

            return await DbContext.SaveChangesAsync();
        }

    }
}