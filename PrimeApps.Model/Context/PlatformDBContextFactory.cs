using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PrimeApps.Model.Context
{
    public class PlatformDBContextFactory : DbContextFactory<PlatformDBContext>
    {
        protected override PlatformDBContext CreateNewInstance(DbContextOptions<PlatformDBContext> options)
        {
            return new PlatformDBContext(options);
        }
    }
}
