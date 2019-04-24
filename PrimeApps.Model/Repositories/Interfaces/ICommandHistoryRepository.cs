using System.Threading.Tasks;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ICommandHistoryRepository: IRepositoryBaseTenant
    {
        Task<int> Create(CommandHistory history);
    }
}    