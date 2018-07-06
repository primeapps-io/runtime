using PrimeApps.Model.Entities.Application;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
	public interface IComponentRepository : IRepositoryBaseTenant
	{
		Task<List<Components>> GetByType(ComponentType type);
	}
}
