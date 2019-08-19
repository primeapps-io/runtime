using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IHistoryDatabaseRepository : IRepositoryBaseTenant
    {
        Task<List<HistoryDatabase>> GetDiffs(string min);
        Task<HistoryDatabase> Get(Guid commandId);
        Task<HistoryDatabase> Get(string tag);
        Task<HistoryDatabase> GetLast();
        Task<int> Update(HistoryDatabase historyDatabase);
        Task<int> Create(HistoryDatabase historyDatabase);
    }
}