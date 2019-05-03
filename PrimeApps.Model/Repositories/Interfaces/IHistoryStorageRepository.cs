using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IHistoryStorageRepository: IRepositoryBaseTenant
    {
        Task<int> Create(HistoryStorage historyStorage);
    }
}    