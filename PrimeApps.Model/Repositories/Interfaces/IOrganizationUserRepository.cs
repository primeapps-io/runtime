using PrimeApps.Model.Common;
using PrimeApps.Model.Enums;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IOrganizationUserRepository : IRepositoryBaseStudio
    {
        Task<OrganizationUser> Get(int userId, int organizationId);
        Task<List<OrganizationUser>> GetByOrganizationId(int organizationId);
        Task<OrganizationRole> GetUserRole(int userId, int organizationId); 
        Task<int> Create(OrganizationUser user);
        Task<int> Delete(OrganizationUser user);
        Task<int> Update(OrganizationUser user);
    }
}
