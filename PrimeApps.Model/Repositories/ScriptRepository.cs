using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Common;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
    public class ScriptRepository : RepositoryBaseTenant, IScriptRepository
    {
        public ScriptRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
        {
        }

        public async Task<int> Count()
        {
            return await DbContext.Components
                .Where(x => !x.Deleted && x.Type == ComponentType.Script).CountAsync();
        }

        public async Task<Component> Get(int id)
        {
            return await DbContext.Components
                .Where(x => !x.Deleted && x.Id == id && x.Type == ComponentType.Script)
                .FirstOrDefaultAsync();
        }

        public async Task<Component> GetByName(string name)
        {
            return await DbContext.Components.Where(x => !x.Deleted && x.Name == name && x.Type == ComponentType.Script).FirstOrDefaultAsync();
        }

        public async Task<bool> IsUniqueName(string name)
        {
            return await DbContext.Components
                .Where(x => !x.Deleted && x.Name == name && x.Type == ComponentType.Script)
                .AnyAsync();
        }

        public async Task<IQueryable<Component>> Find()
        {
            var components = DbContext.Components 
                .Where(x => !x.Deleted && x.Type == ComponentType.Script && x.Place != ComponentPlace.GlobalConfig);

            return components;
        }

        public async Task<List<Component>> GetByPlace(ComponentPlace place)
        {
            return await DbContext.Components
                .Where(x => !x.Deleted && x.Type == ComponentType.Script && x.Place == place).ToListAsync();
        }

        public async Task<Component> GetGlobalSettings()
        {
            return await DbContext.Components
                .Where(x => !x.Deleted && x.Type == ComponentType.Script && x.Place == ComponentPlace.GlobalConfig)
                .FirstOrDefaultAsync();
        }

        public async Task<int> Create(Component component)
        {
            DbContext.Components.Add(component);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Component component)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(Component component)
        {
            component.Deleted = true;
            return await DbContext.SaveChangesAsync();
        }
    }
}