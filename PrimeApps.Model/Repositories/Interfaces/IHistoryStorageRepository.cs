using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IHistoryStorageRepository : IRepositoryBaseTenant
    {
        Task<List<HistoryStorage>> GetDiffs(string min);
        Task<HistoryStorage> GetLast();
        Task<int> Create(HistoryStorage historyStorage);
        Task<int> Update(HistoryStorage historyDatabase);
    }
}