using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class ComponentRepository : RepositoryBaseTenant, IComponentRepository
    {
        public ComponentRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

        public async Task<int> Count()
        {
            return await DbContext.Components
               .Where(x => !x.Deleted).CountAsync();
        }

        public async Task<Component> Get(int id)
        {
            return await DbContext.Components
               .Where(x => !x.Deleted && x.Id == id)
               .FirstOrDefaultAsync();
        }

        public async Task<ICollection<Component>> Find(PaginationModel paginationModel)
        {
            var components = await DbContext.Components
                .Where(x => !x.Deleted)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit)
                .ToListAsync();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Module).GetProperty(paginationModel.OrderColumn);

                if (paginationModel.OrderType == "asc")
                {
                    components = components.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    components = components.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }

            }

            return components;
        }

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

        public async Task<Component> GetGlobalSettings()
        {
            return await DbContext.Components
                .Where(x => !x.Deleted && x.Place == ComponentPlace.GlobalConfig)
                .FirstOrDefaultAsync();
        }

        public async Task<int> Create(Component component)
        {
            DbContext.Components.Add(component);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(Component organization)
        {
            return await DbContext.SaveChangesAsync();
        }
        public async Task<int> Delete(Component organization)
        {
            organization.Deleted = true;
            return await DbContext.SaveChangesAsync();
        }
    }
}
