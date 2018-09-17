using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Entities.Application;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IBpmRepository : IRepositoryBaseTenant
    {
        Task<BpmWorkflow> Get(int id);
        Task<ICollection<BpmWorkflow>> Find(BpmFindRequest request);
        Task<int> Count(BpmFindRequest request);
        Task<int> Create(BpmWorkflow note);
        Task<int> Update(BpmWorkflow note);
        Task<int> DeleteSoft(BpmWorkflow note);
        Task<int> DeleteHard(BpmWorkflow note);
    }
}
