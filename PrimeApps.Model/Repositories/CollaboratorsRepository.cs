using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;

namespace PrimeApps.Model.Repositories
{
    public class CollaboratorsRepository : RepositoryBaseStudio, ICollaboratorsRepository
    {
        public CollaboratorsRepository(StudioDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration)
        {
        }

        public async Task<List<AppCollaborator>> GetByAppId(int appId)
        {
            return await DbContext.AppCollaborators
                .Where(x => !x.Deleted && x.AppId == appId).ToListAsync();
        }

        public List<AppCollaborator> GetByUserId(int userId, int organizationId, int? appId)
        {
            var teamIds = DbContext.TeamUsers.Where(x => x.UserId == userId && !x.Team.Deleted).Select(x => x.TeamId).ToList();

            return  DbContext.AppCollaborators
                .Include(x => x.Team)
                .Where(x => !x.Deleted && x.AppDraft.OrganizationId == organizationId && x.AppId == appId && (x.UserId == userId || (x.Team != null && teamIds.Contains((int)x.TeamId))))
                .ToList();
        }

        public async Task<int> AppCollaboratorAdd(AppCollaborator appCollaborator)
        {
            DbContext.AppCollaborators.Add(appCollaborator);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(AppCollaborator appCollaborator)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<AppCollaborator> GetById(int id)
        {
            return await DbContext.AppCollaborators.Where(x => x.Id == id && !x.Deleted)
                .FirstOrDefaultAsync();
        }

        public async Task<int> Delete(AppCollaborator appCollaborator)
        {
            appCollaborator.Deleted = true;
            return await DbContext.SaveChangesAsync();
        }
    }
}