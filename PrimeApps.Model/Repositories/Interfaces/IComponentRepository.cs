using PrimeApps.Model.Entities.Tenant;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories.Interfaces
{
	public interface IComponentRepository : IRepositoryBaseTenant
	{
		Task<List<Component>> GetByType(ComponentType type);
	}
}
