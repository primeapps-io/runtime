﻿using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PrimeApps.Model.Repositories
{
	public class ComponentRepository : RepositoryBaseTenant, IComponentRepository
	{
		public ComponentRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

		public async Task<List<Components>> GetByType(ComponentType type)
		{
			var components = await DbContext.Components
				.Where(x => !x.Deleted && x.Type == type).ToListAsync();

			return components;
		}
	}
}
