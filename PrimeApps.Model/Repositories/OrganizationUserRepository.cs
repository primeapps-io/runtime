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
    public class OrganizationUserRepository : RepositoryBaseConsole, IOrganizationUserRepository
    {
        public OrganizationUserRepository(ConsoleDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration) { }

        public async Task<OrganizationUser> Get(int userId, int organizationId)
        {
            return await DbContext.OrganizationUsers
                .Where(x => x.OrganizationId == organizationId && x.UserId == userId && !x.Organization.Deleted)
                .FirstOrDefaultAsync();
        }

        public async Task<List<OrganizationUser>> GetByOrganizationId(int organizationId)
        {
            return await DbContext.OrganizationUsers
                .Where(x => x.OrganizationId == organizationId && !x.Organization.Deleted)
                .Include(x => x.ConsoleUser)
                .ToListAsync();
        }

        public async Task<OrganizationRole> GetUserRole(int userId, int organizationId)
        {
            return await DbContext.OrganizationUsers
                .Where(x => x.UserId == userId && x.OrganizationId == organizationId && !x.Organization.Deleted)
                .Select(x => x.Role)
                .FirstOrDefaultAsync();
        }
         
        public async Task<int> Create(OrganizationUser user)
        {
            DbContext.OrganizationUsers.Add(user);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(OrganizationUser user)
        {
            DbContext.OrganizationUsers.Remove(user);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(OrganizationUser user)
        {
            return await DbContext.SaveChangesAsync();
        }
    }
}
