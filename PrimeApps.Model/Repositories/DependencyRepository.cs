﻿using System;
using System.Collections.Generic;
using System.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Repositories.Interfaces;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hangfire;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Common;
using PrimeApps.Model.Common.Note;
using PrimeApps.Model.Helpers;
using PrimeApps.Model.Enums;

namespace PrimeApps.Model.Repositories
{
	public class DependencyRepository : RepositoryBaseTenant, IDependencyRepository
	{
		private Warehouse _warehouse;

		public DependencyRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

		public DependencyRepository(TenantDBContext dbContext, Warehouse warehouse, IConfiguration configuration) : base(dbContext, configuration)
		{
			_warehouse = warehouse;
		}

		public async Task<int> Count(int id)
		{
			var count = DbContext.Dependencies
			   .Where(x => !x.Deleted);

			if (id > 0)
				count = count.Where(x => x.ModuleId == id);

			return await count.CountAsync();
		}

        public ICollection<Dependency> Find(int id, PaginationModel paginationModel)
        {
            var dependencies = GetPaginationGQuery(id, paginationModel)
                .Skip(paginationModel.Offset * paginationModel.Limit)
                .Take(paginationModel.Limit).ToList();

            if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
            {
                var propertyInfo = typeof(Module).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

                if (paginationModel.OrderType == "asc")
                {
                    dependencies = dependencies.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
                }
                else
                {
                    dependencies = dependencies.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
                }

            }

            return dependencies;

        }

        public async Task<Dependency> GetById(int id)
		{
			var dependency = await GetDependencyQuery()
								.Include(x => x.Module).ThenInclude(module => module.Sections)
								//.Include(x => x.Module).ThenInclude(module => module.Fields)
								.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

			return dependency;
		}

		public async Task<ICollection<Dependency>> GetAll()
		{
			var dependencies = await GetDependencyQuery()
				.Where(x => !x.Deleted)
				.ToListAsync();

			return dependencies;
		}

		public async Task<ICollection<Dependency>> GetAllDeleted()
		{
			var dependencies = await GetDependencyQuery()
				.Where(x => x.Deleted)
				.ToListAsync();

			return dependencies;
		}

		public async Task<Dependency> GetDependency(int id)
		{
			var dependency = await DbContext.Dependencies
				.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

			return dependency;
		}

		private IQueryable<Dependency> GetDependencyQuery()
		{
			return DbContext.Dependencies
				.Include(dependency => dependency.Module)
				.ThenInclude(x => x.Fields);
		}

		private IQueryable<Dependency> GetPaginationGQuery(int id, PaginationModel paginationModel, bool withIncludes = true)
		{
			var dependencies = DbContext.Dependencies
				.Where(dependency => !dependency.Deleted);

			if (id > 0)
				dependencies = dependencies.Where(x => x.ModuleId == id);

			dependencies = dependencies//.Include(dependency => dependency.Module).ThenInclude(module => module.Sections)
						  .Include(dependency => dependency.Module).ThenInclude(module => module.Fields);

			return dependencies.OrderByDescending(x => x.Id);

		}
	}
}