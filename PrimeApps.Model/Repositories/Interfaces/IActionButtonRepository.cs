using PrimeApps.Model.Entities.Application;
using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Common.ActionButton;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IActionButtonRepository : IRepositoryBaseTenant
    {
        Task<ICollection<ActionButtonViewModel>> GetByModuleId(int id);
        Task<ActionButton> GetByIdBasic(int id);
        Task<ActionButton> GetById(int id);
        Task<int> Create(ActionButton actionbutton);
        Task<int> Update(ActionButton actionbutton);
        Task<int> DeleteSoft(ActionButton actionbutton);
    }
}
