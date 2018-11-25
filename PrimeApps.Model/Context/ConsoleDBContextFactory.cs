using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PrimeApps.Model.Context
{
    public class ConsoleDBContextFactory : DbContextFactory<ConsoleDBContext>
    {
        protected override ConsoleDBContext CreateNewInstance(DbContextOptions<ConsoleDBContext> options)
        {
            return new ConsoleDBContext(options);
        }
    }
}
