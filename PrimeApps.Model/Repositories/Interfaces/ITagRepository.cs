using System.Collections.Generic;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Application;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ITagRepository : IRepositoryBaseTenant
    {
        Task<ICollection<Tag>> GetAllBasic();
        Task<ICollection<Tag>> GetByFieldId(int id);
        Task<Tag> GetById(int id);
        Task<int> Create(Tag tag);
    }
}
