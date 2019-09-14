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
		public ComponentRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration)
		{
		}

		public async Task<int> Count()
		{
			return await DbContext.Components
				.Where(x => !x.Deleted && x.Type == ComponentType.Component).CountAsync();
		}

		public async Task<Component> Get(int id)
		{
			return await DbContext.Components
			   .Where(x => !x.Deleted && x.Id == id && x.Type == ComponentType.Component)
			   .FirstOrDefaultAsync();
		}

		public async Task<Component> Get(string name)
		{
			return await DbContext.Components
				.Where(x => !x.Deleted && x.Name == name && x.Type == ComponentType.Component)
				.FirstOrDefaultAsync();
		}

		public async Task<ICollection<Component>> Find(PaginationModel paginationModel)
		{
			var components = DbContext.Components
				.Where(x => !x.Deleted && x.Type == ComponentType.Component)
				.Skip(paginationModel.Offset * paginationModel.Limit)
				.Take(paginationModel.Limit);

			if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
			{
				var propertyInfo = typeof(Module).GetProperty(paginationModel.OrderColumn);

				if (paginationModel.OrderType == "asc")
				{
					components = components.OrderBy(x => propertyInfo.GetValue(x, null));
				}
				else
				{
					components = components.OrderByDescending(x => propertyInfo.GetValue(x, null));
				}
			}

			return await components.ToListAsync();
		}

		public async Task<List<Component>> GetByType(ComponentType type)
		{
			var components = await DbContext.Components
				.Where(x => !x.Deleted && x.Type == type).ToListAsync();

			return components;
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

		public async Task<Component> GetGlobalConfig()
		{
			return await DbContext.Components
				.Where(x => !x.Deleted && x.Type == ComponentType.Script && x.Place == ComponentPlace.GlobalConfig)
				.SingleOrDefaultAsync();
		}
	}
}