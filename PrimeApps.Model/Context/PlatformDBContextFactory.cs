using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Model.Context
{
    public class PlatformDBContextFactory : DbContextFactory<PlatformDBContext>
    {
        protected override PlatformDBContext CreateNewInstance(DbContextOptions<PlatformDBContext> options, IConfiguration configuration)
        {
            return new PlatformDBContext(options, configuration);
        }
    }
}