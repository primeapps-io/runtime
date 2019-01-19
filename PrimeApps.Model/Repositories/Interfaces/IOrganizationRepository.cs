using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IOrganizationRepository : IRepositoryBaseConsole
    {
        bool IsOrganizationAvaliable(int userId, int organizationId);
        Task<bool> IsOrganizationNameAvailableAsync(string name);
        Task<Organization> Get(int userId, int organizationId);
        Task<List<OrganizationUser>> GetUsersByOrganizationId(int organizationId);
        Task<List<Organization>> GetWithUsers(int id);
        Task<List<Organization>> GetByUserId(int userId);
        Task<List<Organization>> GetWithTeams(int id);
        Task<Organization> GetAll(int userId, int organizationId);
        Task<int> Create(Organization organization);
        Task<int> Delete(Organization organization);
        Task<int> Update(Organization organization);
    }
}
