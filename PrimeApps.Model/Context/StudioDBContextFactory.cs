using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Model.Context
{
    public class StudioDBContextFactory : DbContextFactory<StudioDBContext>
    {
        protected override StudioDBContext CreateNewInstance(DbContextOptions<StudioDBContext> options, IConfiguration configuration)
        {
            return new StudioDBContext(options, configuration);
        }
    }
}