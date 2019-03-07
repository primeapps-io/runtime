using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.EntityFrameworkCore;

namespace PrimeApps.Model.Context
{
    public class StudioDBContextFactory : DbContextFactory<StudioDBContext>
    {
        protected override StudioDBContext CreateNewInstance(DbContextOptions<StudioDBContext> options)
        {
            return new StudioDBContext(options);
        }
    }
}
