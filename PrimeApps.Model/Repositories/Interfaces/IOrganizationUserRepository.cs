using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IOrganizationUserRepository : IRepositoryBaseConsole
    {
        Task<List<OrganizationUser>> GetByOrganizationId(int organizationId);
        Task<List<OrganizationUser>> GetByUserId(int userId);
        Task<int> Create(OrganizationUser user);
        Task<int> Delete(OrganizationUser user);
        Task<int> Update(OrganizationUser user);
    }
}
