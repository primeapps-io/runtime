using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.Bpm;
using PrimeApps.Model.Entities.Tenant;

namespace PrimeApps.Model.Repositories
{
    public interface IBpmCategoryRepository
    {
        Task<int> Count(BpmFindRequest request);
        Task<int> Create(BpmCategory BpmCategory);
        Task<int> DeleteHard(BpmCategory BpmCategory);
        Task<int> DeleteSoft(BpmCategory BpmCategory);
        Task<BpmCategory> Get(int id);
        Task<ICollection<BpmCategory>> GetAll();
        Task<int> Update(BpmCategory BpmCategory);
    }
}