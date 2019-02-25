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
    public class StudioUserRepository : RepositoryBaseStudio, IStudioUserRepository
    {
        public StudioUserRepository(StudioDBContext dbContext, IConfiguration configuration)
           : base(dbContext, configuration) { }

        public async Task<int> Create(StudioUser user)
        {
            DbContext.Users.Add(user);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(StudioUser user)
        {
            DbContext.Users.Remove(user);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(StudioUser user)
        {
            return await DbContext.SaveChangesAsync();
        }
    }
}
