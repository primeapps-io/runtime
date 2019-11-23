using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace PrimeApps.Model.Context
{
    public class TenantDBContextFactory : DbContextFactory<TenantDBContext>
    {
        protected override TenantDBContext CreateNewInstance(DbContextOptions<TenantDBContext> options, IConfiguration configuration)
        {
            return new TenantDBContext(options, configuration);
        }
    }
}