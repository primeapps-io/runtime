using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PrimeApps.Model.Context
{
    public class TenantDBContextFactory : DbContextFactory<TenantDBContext>
    {
        protected override TenantDBContext CreateNewInstance(DbContextOptions<TenantDBContext> options)
        {
            return new TenantDBContext(options);
        }
    }
}
