using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class CommandHistoryRepository: RepositoryBaseTenant, ICommandHistoryRepository
    {
        public CommandHistoryRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }
        
        public async Task<int> Create(CommandHistory history)
        {
            DbContext.CommandHistories.Add(history);

            return await DbContext.SaveChangesAsync();
        }

    }
}