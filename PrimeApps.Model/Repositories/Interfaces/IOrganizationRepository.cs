using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IOrganizationRepository : IRepositoryBaseStudio
    {
        bool IsOrganizationAvaliable(int userId, int organizationId);
        Task<bool> IsOrganizationNameAvailable(string name);
        Task<Organization> Get(int organizationId);
        Task<Organization> Get(int userId, int organizationId);
        Task<List<OrganizationUser>> GetUsersByOrganizationId(int organizationId);
        Task<List<Organization>> GetWithUsers(int id);
        Task<List<OrganizationUser>> GetByUserId(int userId);
        Task<List<Organization>> GetWithTeams(int id);
        Task<Organization> GetAll(int userId, int organizationId);
        Task<int> Create(Organization organization);
        Task<int> Delete(Organization organization);
        Task<int> Update(Organization organization);
    }
}
