using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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
    public class OrganizationRepository : RepositoryBaseStudio, IOrganizationRepository
    {
        public OrganizationRepository(StudioDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration) { }

        public bool IsOrganizationAvaliable(int userId, int organizationId)
        {
            var orgId = DbContext.OrganizationUsers
                .Where(x => x.UserId == userId && x.OrganizationId == organizationId && !x.Organization.Deleted)
                .Select(x => x.OrganizationId)
                .FirstOrDefault();

            return orgId != 0;
        }

        public async Task<bool> IsOrganizationNameAvailable(string name)
        {
            return await DbContext.Organizations
                .Where(x => x.Name == name)
                .FirstOrDefaultAsync() == null;
        }
        
        public async Task<Organization> Get(int organizationId)
        {
            return await DbContext.OrganizationUsers
                .Where(x => x.OrganizationId == organizationId && !x.Organization.Deleted)
                .Select(x => x.Organization)
                .FirstOrDefaultAsync();
        }

        public async Task<Organization> Get(int userId, int organizationId)
        {
            return await DbContext.OrganizationUsers
                .Where(x => x.UserId == userId && x.OrganizationId == organizationId && !x.Organization.Deleted)
                .Select(x => x.Organization)
                .FirstOrDefaultAsync();
        }

        public async Task<List<OrganizationUser>> GetUsersByOrganizationId(int organizationId)
        {
            return await DbContext.OrganizationUsers
                .Where(x => x.OrganizationId == organizationId && !x.Organization.Deleted)
                .ToListAsync();
        }

        public async Task<List<OrganizationUser>> GetByUserId(int userId)
        {
            return await DbContext.OrganizationUsers
                .Include(x => x.Organization)
                .Where(x => x.UserId == userId && !x.Organization.Deleted)
                .ToListAsync();
        }

        public async Task<List<Organization>> GetWithUsers(int id)
        {
            return await DbContext.Organizations
                .Include(x => x.OrganizationUsers)
                .Where(x => !x.Deleted && x.Id == id)
                .ToListAsync();
        }

        public async Task<List<Organization>> GetWithTeams(int id)
        {
            return await DbContext.Organizations
                .Include(x => x.Teams)
                .Where(x => !x.Deleted && x.Id == id)
                .ToListAsync();
        }

        public async Task<Organization> GetAll(int userId, int organizationId)
        {
            var organization = await DbContext.OrganizationUsers
                .Include(x => x.Organization)
                    .ThenInclude(x => (x.Teams as Team).TeamUsers)
                .Include(x => x.Organization)
                    .ThenInclude(x => x.OrganizationUsers)
                .Where(x => x.OrganizationId == organizationId && x.UserId == userId && !x.Organization.Deleted)
                .FirstOrDefaultAsync();

            return organization.Organization;
        }

        public async Task<int> Create(Organization organization)
        {
            DbContext.Organizations.Add(organization);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(Organization organization)
        {
            organization.Deleted = true;
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Organization organization)
        {
            return await DbContext.SaveChangesAsync();
        }
    }
}
