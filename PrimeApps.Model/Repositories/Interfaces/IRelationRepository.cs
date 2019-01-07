using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;
using PrimeApps.Model.Common;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IRelationRepository : IRepositoryBaseTenant
    {
        Task<Relation> GetById(int id);
         Task<ICollection<Relation>> GetAll();
        Task<ICollection<Relation>> GetAllDeleted();
        Task<Relation> GetRelation(int id);
        Task<int> Count();
        Task<ICollection<Relation>> Find(PaginationModel paginationModel);
    }
}
