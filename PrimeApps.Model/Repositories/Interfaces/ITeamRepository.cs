using PrimeApps.Model.Common;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;
using System.Linq;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ITeamRepository : IRepositoryBaseStudio
    {
        Task<int> Count(int organizationId);
        Task<List<Team>> GetAll(int organizationId);
        Task<Team> GetByTeamId(int id);
        Task<List<Team>> GetByUserId(int userId);
        Task<Team> GetByName(string name, int organizationId);
        Task<List<Team>> GetByOrganizationId(int organizationId);
        IQueryable<Team> Find(int organizationId);
        Task<int> Create(Team team);
        Task<int> Delete(Team team);
        Task<int> Update(Team team);

        //Team Users

        Task<int> UserTeamAdd(TeamUser teamUser);
        Task<int> UserTeamDelete(TeamUser teamUser);
        Task<TeamUser> GetTeamUser(int UserId, int teamId);
    }
}
