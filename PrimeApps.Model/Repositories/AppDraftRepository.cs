using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Console;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class AppDraftRepository : RepositoryBaseConsole, IAppDraftRepository
    {
        public AppDraftRepository(ConsoleDBContext dbContext, IConfiguration configuration)
           : base(dbContext, configuration) { }

        public async Task<List<AppDraft>> GetByOrganizationId(int organizationId)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .Where(x => x.OrganizationId == organizationId && !x.Deleted)
                .ToListAsync();
        }

        public async Task<List<AppDraft>> GetAll(int userId, string search = "", int page = 0)
        {
            var teamIds = await DbContext.TeamUsers.Where(x => x.UserId == userId && !x.Team.Deleted).Select(x => x.TeamId).ToListAsync();

            var appCollabrator = await DbContext.AppCollaborators
                .Where(x => !x.Deleted && (x.UserId == userId || (x.Team != null && teamIds.Contains((int)x.TeamId))) && (!string.IsNullOrEmpty(search) ? x.AppDraft.Label.Contains(search) : true))
                .Select(x => x.AppDraft)
                .Skip(50 * page)
                .Take(50)
                .Distinct()
                .ToListAsync();

            return appCollabrator;
        }
    }
}
