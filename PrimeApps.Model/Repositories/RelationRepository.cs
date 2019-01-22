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
	public class RelationRepository : RepositoryBaseTenant, IRelationRepository
	{
		private Warehouse _warehouse;

		public RelationRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

		public RelationRepository(TenantDBContext dbContext, Warehouse warehouse, IConfiguration configuration) : base(dbContext, configuration)
		{
			_warehouse = warehouse;
		}

		public async Task<int> Count(int id)
		{
			var count = DbContext.Relations
						.Where(x => !x.Deleted);

			if (id > 0)
				count = count.Where(x => x.ModuleId == id);

			return await count.CountAsync();
		}

		public async Task<ICollection<Relation>> Find(int id, PaginationModel paginationModel)
		{
			var relations = GetPaginationGQuery(id, paginationModel)
				.Skip(paginationModel.Offset * paginationModel.Limit)
				.Take(paginationModel.Limit).ToList();

			if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
			{
				var propertyInfo = typeof(Module).GetProperty(paginationModel.OrderColumn);

				if (paginationModel.OrderType == "asc")
				{
					relations = relations.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
				}
				else
				{
					relations = relations.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
				}

			}

			return relations;
		}

		public async Task<Relation> GetById(int id)
		{
			var relation = await GetRelationQuery()
				.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

			return relation;
		}

		public async Task<ICollection<Relation>> GetAll()
		{
			var relations = await GetRelationQuery()
				.Where(x => !x.Deleted)
				.ToListAsync();

			return relations;
		}

		public async Task<ICollection<Relation>> GetAllDeleted()
		{
			var relations = await GetRelationQuery()
				.Where(x => x.Deleted)
				.ToListAsync();

			return relations;
		}

		public async Task<Relation> GetRelation(int id)
		{
			var relation = await DbContext.Relations
				.FirstOrDefaultAsync(x => x.Id == id && !x.Deleted);

			return relation;
		}

		private IQueryable<Relation> GetRelationQuery()
		{
			return DbContext.Relations
				.Include(relation => relation.ParentModule);
		}

		private IQueryable<Relation> GetPaginationGQuery(int id, PaginationModel paginationModel, bool withIncludes = true)
		{
			var relations = DbContext.Relations
							.Where(relation => !relation.Deleted);
			if (id > 0)
				relations = relations.Where(x => x.ModuleId == id);

			relations = relations.Include(relation => relation.ParentModule)
								  .OrderByDescending(x => x.Id);

			return relations;
		
		}
	}
}