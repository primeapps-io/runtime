using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories
{
    public class TeamRepository : RepositoryBaseStudio, ITeamRepository
    {
        public TeamRepository(StudioDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration) { }

        public async Task<int> Count(int organizationId)
        {
            var count = (await GetByOrganizationId(organizationId)).Count();

            return count;
        }

        public async Task<List<Team>> GetAll(int organizationId)
        {
            return await DbContext.Teams.Include(x => x.Organization)
                .Include(x => x.TeamUsers)
                .Where(x => !x.Deleted && x.OrganizationId == organizationId)
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
                .Where(x => !x.Deleted && (x.TeamUsers as TeamUser).UserId == userId)
                .ToListAsync();
        }

        public async Task<Team> GetByName(string name, int organizationId)
        {
            return await DbContext.Teams.Where(x => x.Name == name && !x.Deleted && x.OrganizationId == organizationId).FirstOrDefaultAsync();
        }

        public async Task<List<Team>> GetByOrganizationId(int organizationId)
        {
            return await DbContext.Teams
                .Include(x => x.TeamUsers)
                .Where(x => x.OrganizationId == organizationId && !x.Deleted)
                .OrderByDescending(x => x.Id)
                .ToListAsync();
        }

        public IQueryable<Team> Find(int organizationId)
        {
            var teams = DbContext.Teams
                .Include(x => x.TeamUsers)
                .Include(x=>x.Organization)
                .Where(x => x.OrganizationId == organizationId && !x.Deleted)
                .OrderByDescending(x => x.Id);

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
