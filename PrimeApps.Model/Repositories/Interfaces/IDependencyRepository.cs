using System.Collections.Generic;
using PrimeApps.Model.Entities.Tenant;
using System.Threading.Tasks;
using PrimeApps.Model.Common;
using PrimeApps.Model.Helpers;

namespace PrimeApps.Model.Repositories.Interfaces
{
	public interface IDependencyRepository : IRepositoryBaseTenant
	{
		Task<Dependency> GetById(int id);
		Task<ICollection<Dependency>> GetAll();
		Task<ICollection<Dependency>> GetAllDeleted();
		Task<Dependency> GetDependency(int id);
		Task<int> Count(int id);
		Task<ICollection<Dependency>> Find(int id, PaginationModel paginationModel);
	}
}
