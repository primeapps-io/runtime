using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Application;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class UserCustomShareRepository : RepositoryBaseTenant, IUserCustomShareRepository
    {
        public UserCustomShareRepository(TenantDBContext dbContext) : base(dbContext) { }

        public async Task<ICollection<UserCustomShare>> GetAllBasic()
        {
            var userowners = await DbContext.UserCustomShares
                .Where(x => !x.Deleted)
                .ToListAsync();

            return userowners;
        }

        public async Task<UserCustomShare> GetByUserId(int id)
        {
            var userowner = await DbContext.UserCustomShares
                .FirstOrDefaultAsync(x => !x.Deleted && x.UserId == id);

            return userowner;
        }

        public async Task<UserCustomShare> GetById(int id)
        {
            var userowner = await DbContext.UserCustomShares
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return userowner;
        }

        public async Task<UserCustomShare> GetByIdBasic(int id)
        {
            var userowner = await DbContext.UserCustomShares
                .FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

            return userowner;
        }

        public async Task<int> Create(UserCustomShare userowner)
        {
            DbContext.UserCustomShares.Add(userowner);

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(UserCustomShare userowner)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> DeleteSoft(UserCustomShare userowner)
        {
            userowner.Deleted = true;

            return await DbContext.SaveChangesAsync();
        }

    }
}
