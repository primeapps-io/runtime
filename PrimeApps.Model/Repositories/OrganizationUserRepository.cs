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
    public class OrganizationUserRepository : RepositoryBaseConsole, IOrganizationUserRepository
    {
        public OrganizationUserRepository(ConsoleDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration) { }

        public async Task<List<OrganizationUser>> GetByOrganizationId(int organizationId)
        {
            return await DbContext.OrganizationUsers
                .Include(x => x.ConsoleUser)
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
