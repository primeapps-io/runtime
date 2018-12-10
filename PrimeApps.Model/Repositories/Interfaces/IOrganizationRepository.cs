using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface IOrganizationRepository : IRepositoryBaseConsole
    {
        List<Organization> Get(int userId, int organizationId);
        Task<List<Organization>> GetWithUsers(int id);
        Task<List<Organization>> GetByUserId(int userId);
        Task<List<Organization>> GetWithTeams(int id);
        Task<Organization> GetAll(int organizationId, int userId);
        Task<int> Create(Organization organization);
        Task<int> Delete(Organization organization);
    }
}
