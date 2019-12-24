using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;
using System.Linq;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IViewRepository : IRepositoryBaseTenant
    {
        Task<View> GetById(int id);
        Task<ICollection<View>> GetAll(int moduleId);
        Task<ICollection<ViewState>> GetAllViewStates(int moduleId);
        Task<ICollection<View>> GetAll();
        Task<int> Create(View view);
        Task<int> Update(View view, List<int> currentFieldIds, List<int> currentFilterIds);
        Task<int> DeleteSoft(View view);
        Task<int> DeleteHard(View view);
        Task<int> DeleteHardViewState(ViewState viewState);
        Task<int> DeleteViewField(int id, string name);
        Task<ViewState> GetViewState(int moduleId, int userId);
        Task<int> CreateViewState(ViewState viewState);
        Task<int> UpdateViewState(ViewState viewState);
        Task<int> DeleteViewShare(ViewShares view, TenantUser user); 
        Task<int> Count(int id);
        IQueryable<View> Find(int id);
    }
}
