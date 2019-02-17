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
    public class ConsoleUserRepository : RepositoryBaseConsole, IConsoleUserRepository
    {
        public ConsoleUserRepository(StudioDBContext dbContext, IConfiguration configuration)
           : base(dbContext, configuration) { }

        public async Task<int> Create(ConsoleUser user)
        {
            DbContext.Users.Add(user);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(ConsoleUser user)
        {
            DbContext.Users.Remove(user);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(ConsoleUser user)
        {
            return await DbContext.SaveChangesAsync();
        }
    }
}
