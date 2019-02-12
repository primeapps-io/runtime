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
    public class AppProfileRepository : RepositoryBaseConsole, IAppProfileRepository
    {
        public AppProfileRepository(ConsoleDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<int> Create(AppProfile appProfile)
        {
            DbContext.AppProfiles.Add(appProfile);
            return await DbContext.SaveChangesAsync();
        }
        public async Task<int> Update(AppProfile appProfile)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(AppProfile appProfile)
        {
            appProfile.Deleted = true;
            return await DbContext.SaveChangesAsync();
        }

        public async Task<AppProfile> GetByAppId(int id)
        {
            return await DbContext.AppProfiles.Where(x => x.AppId == id && !x.Deleted)
                .FirstOrDefaultAsync();
        }
    }
}
