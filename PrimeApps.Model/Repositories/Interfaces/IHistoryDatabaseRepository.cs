using System;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IHistoryDatabaseRepository : IRepositoryBaseTenant
    {
        Task<HistoryDatabase> Get(Guid commandId);
        Task<HistoryDatabase> GetLast();
        Task<int> Update(HistoryDatabase historyDatabase);
        Task<int> Create(HistoryDatabase historyDatabase);
    }
}