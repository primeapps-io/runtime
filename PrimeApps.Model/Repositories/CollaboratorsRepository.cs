using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Console;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class CollaboratorsRepository : RepositoryBaseConsole
    {
        public CollaboratorsRepository(ConsoleDBContext dbContext, IConfiguration configuration) 
            : base(dbContext, configuration) { }

        public async Task<List<AppCollaborator>> GetByAppId(int appId)
        {
            return await DbContext.AppCollaborators
                .Where(x => !x.Deleted && x.AppId == appId).ToListAsync();
        }

        public async Task<List<AppCollaborator>> GetByUserId(int userId)
        {
            return await DbContext.AppCollaborators
                .Include(x => x.Team)
                .Where(x => !x.Deleted && (x.UserId == userId) /*|| x.Team.TeamUsers.Contains(userId)*/).ToListAsync();
        }
    }
}
