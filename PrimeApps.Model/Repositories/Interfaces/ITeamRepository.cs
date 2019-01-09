using PrimeApps.Model.Common;
using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories.Interfaces
{
    public interface ITeamRepository : IRepositoryBaseConsole
    {
        Task<int> Count(int organizationId);
        Task<List<Team>> GetAll();
        Task<Team> GetByTeamId(int id);
        Task<List<Team>> GetByUserId(int userId);
        Task<Team> GetByName(string name);
        Task<List<Team>> GetByOrganizationId(int organizationId);
        Task<ICollection<Team>> Find(PaginationModel paginationModel, int organizationId);
        Task<int> Create(Team team);
        Task<int> Delete(Team team);
        Task<int> Update(Team team);

        //Team Users

        Task<int> UserTeamAdd(TeamUser teamUser);
        Task<int> UserTeamDelete(TeamUser teamUser);
        Task<TeamUser> GetTeamUser(int UserId, int teamId);
    }
}
