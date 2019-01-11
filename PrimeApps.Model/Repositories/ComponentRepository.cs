using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories
{
    public class ComponentRepository : RepositoryBaseTenant, IComponentRepository
    {
        public ComponentRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<List<Component>> GetByType(ComponentType type)
        {
            var components = await DbContext.Components
                .Where(x => !x.Deleted && x.Type == type).ToListAsync();

            return components;
        }

        public async Task<List<Component>> GetByPlace(ComponentPlace place)
        {
            return await DbContext.Components
                .Where(x => !x.Deleted && x.Place == place).ToListAsync();
        }

        public async Task<Component> GetGlobalConfig()
        {
            return await DbContext.Components
                .Where(x => !x.Deleted && x.Place == ComponentPlace.GlobalConfig)
                .FirstOrDefaultAsync();
        }
    }
}
