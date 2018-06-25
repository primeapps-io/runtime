using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Repositories.Interfaces;

namespace PrimeApps.Model.Repositories
{
    public class UserGroupRepository : RepositoryBaseTenant, IUserGroupRepository
    {
        public UserGroupRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<UserGroup> GetById(int id)
        {
            var userGroup = await DbContext.UserGroups
                .Include(x => x.Users)
				.ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return userGroup;
        }

        public async Task<UserGroup> GetByName(string name)
        {
            var userGroup = await DbContext.UserGroups
                .Include(x => x.Users)
				.ThenInclude(x => x.User)
                .FirstOrDefaultAsync(x => !x.Deleted && x.Name == name);

            return userGroup;
        }

        public async Task<ICollection<UserGroup>> GetAll()
        {
            var userGroups = DbContext.UserGroups
                .Include(x => x.Users)
				.ThenInclude(x => x.User)
                .Where(x => !x.Deleted)
                .OrderBy(x => x.CreatedAt);

            return await userGroups.ToListAsync();
        }

        public async Task<int> Create(UserGroup userGroup)
        {
            DbContext.UserGroups.Add(userGroup);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(UserGroup userGroup)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(UserGroup userGroup)
        {
            userGroup.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteHard(UserGroup userGroup)
        {
            DbContext.UserGroups.Remove(userGroup);

            return await DbContext.SaveChangesAsync();
        }
    }
}

