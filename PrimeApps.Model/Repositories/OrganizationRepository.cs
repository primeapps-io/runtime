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
    public class OrganizationRepository : RepositoryBaseConsole, IOrganizationRepository
    {
        public OrganizationRepository(ConsoleDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration) { }

        public List<Organization> Get(int userId, int organizationId)
        {
            return DbContext.OrganizationUsers
                .Where(x => x.UserId == userId && x.OrganizationId == organizationId && !x.Organization.Deleted)
                .Select(x => x.Organization)
                .ToList();
        }

        public async Task<List<Organization>> GetByUserId(int userId)
        {
            return await DbContext.OrganizationUsers
                .Include(x => x.Organization)
                .Where(x => x.UserId == userId && !x.Organization.Deleted)
                .Select(x => x.Organization)
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

        public async Task<Organization> GetAll(int organizationId, int userId)
        {
            var organization = await DbContext.OrganizationUsers
                .Include(x => x.Organization)
                    .ThenInclude(x => x.OrganizationUsers)
                .Include(x => x.Organization)
                    .ThenInclude(x => x.Teams)
                .Where(x => x.OrganizationId == organizationId && x.UserId == userId)
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
