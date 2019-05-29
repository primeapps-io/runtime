using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IHistoryStorageRepository : IRepositoryBaseTenant
    {
        Task<HistoryStorage> GetLast();
        Task<int> Create(HistoryStorage historyStorage);
        Task<int> Update(HistoryStorage historyDatabase);
    }
}