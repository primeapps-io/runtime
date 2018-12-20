using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
                .Where(x => !x.Deleted && (x.TeamUsers as TeamUser).UserId == userId)
                .ToListAsync();
        }

        public async Task<List<Team>> GetByOrganizationId(int organizationId)
        {
            return await DbContext.Teams
                .Include(x => x.TeamUsers)
                .Where(x => x.OrganizationId == organizationId && !x.Deleted)
                .ToListAsync();
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
    }
}
