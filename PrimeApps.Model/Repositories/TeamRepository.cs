using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class TeamRepository : RepositoryBaseConsole, ITeamRepository
    {
        public TeamRepository(ConsoleDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration) { }

        public async Task<int> Count(int organizationId)
        {
            var count = (await GetByOrganizationId(organizationId)).Count();

            return count;
        }

        public async Task<List<Team>> GetAll()
        {
            return await DbContext.Teams.Include(x => x.Organization)
                .Include(x => x.TeamUsers)
                .Where(x => !x.Deleted)
               .ToListAsync();
        }

        public async Task<Team> GetByTeamId(int id)
        {
            return await DbContext.Teams.Include(x => x.Organization)
                .Include(x => x.TeamUsers)
                .Where(x => x.Id == id && !x.Deleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<Team>> GetByUserId(int userId)
        {
            return await DbContext.Teams
                .Include(x => x.TeamUsers)
                .Where(x => !x.Deleted)
                .ToListAsync();
        }

        public async Task<Team> GetByName(string name)
        {
            return await DbContext.Teams.Where(x => x.Name == name && !x.Deleted).FirstOrDefaultAsync();
        }

        public async Task<List<Team>> GetByOrganizationId(int organizationId)
        {
            return await DbContext.Teams
                .Include(x => x.TeamUsers)
                .Where(x => x.OrganizationId == organizationId && !x.Deleted).OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public async Task<ICollection<Team>> Find(PaginationModel paginationModel, int organizationId)
        {
            var teams = await GetByOrganizationId(organizationId);
            teams = teams.Skip(paginationModel.Offset * paginationModel.Limit)
            .Take(paginationModel.Limit).ToList();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Team).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    teams = teams.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    teams = teams.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }

            }

            return teams;

        }

        public async Task<int> Create(Team team)
        {
            DbContext.Teams.Add(team);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(Team team)
        {
            team.Deleted = true;
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Team team)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> UserTeamAdd(TeamUser teamUser)
        {
            DbContext.TeamUsers.Add(teamUser);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> UserTeamDelete(TeamUser teamUser)
        {
            DbContext.TeamUsers.Remove(teamUser);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<TeamUser> GetTeamUser(int UserId, int teamId)
        {
            return await DbContext.TeamUsers
                 .Where(x => x.UserId == UserId && x.TeamId == teamId).FirstOrDefaultAsync();

        }
    }
}
