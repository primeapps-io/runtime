﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using PrimeApps.Model.Context;
using PrimeApps.Model.Entities.Tenant;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
	public class ViewRepository : RepositoryBaseTenant, IViewRepository
	{
		public ViewRepository(TenantDBContext dbContext, IConfiguration configuration) : base(dbContext, configuration) { }

		public async Task<View> GetById(int id)
		{
			var view = await DbContext.Views
				.Include(x => x.Fields)
				.Include(x => x.Filters)
				.Include(x => x.Shares)
				.ThenInclude(x => x.User)
				.FirstOrDefaultAsync(x => !x.Deleted && x.Id == id);

			return view;

		}

		public async Task<ICollection<View>> GetAll(int moduleId)
		{
			var views = await DbContext.Views
				.Include(x => x.Fields)
				.Include(x => x.Filters)
				.Include(x => x.Shares)
				.ThenInclude(x => x.User)
				.Where(x => x.ModuleId == moduleId && !x.Deleted)
				.Where(x => x.SharingType == ViewSharingType.Everybody
				|| x.CreatedBy.Id == CurrentUser.UserId
				|| x.Shares.Any(j => j.UserId == CurrentUser.UserId))
				.ToListAsync();

			return views;
		}

		public async Task<ICollection<ViewState>> GetAllViewStates(int moduleId)
		{
			var viewStates = await DbContext.ViewStates
				.Where(x => x.ModuleId == moduleId && !x.Deleted)
				.ToListAsync();

			return viewStates;
		}

		public async Task<ICollection<View>> GetAll()
		{
			var views = await DbContext.Views
				.Include(x => x.Fields)
				.Include(x => x.Filters)
				.Include(x => x.Shares)
				.ThenInclude(x => x.User)
				.Where(x => !x.Deleted)
				.Where(x => x.SharingType == ViewSharingType.Everybody
							|| x.CreatedBy.Id == CurrentUser.UserId
							|| x.Shares.Any(j => j.UserId == CurrentUser.UserId))
				.ToListAsync();

			return views;
		}

		public async Task<int> Create(View view)
		{
			DbContext.Views.Add(view);

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> Update(View view, List<int> currentFieldIds, List<int> currentFilterIds)
		{
			foreach (var fieldId in currentFieldIds)
			{
				var currentField = view.Fields.First(x => x.Id == fieldId);
				view.Fields.Remove(currentField);
				DbContext.ViewFields.Remove(currentField);
			}

			foreach (var filterId in currentFilterIds)
			{
				var currentFilter = view.Filters.First(x => x.Id == filterId);
				view.Filters.Remove(currentFilter);
				DbContext.ViewFilters.Remove(currentFilter);
			}

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> DeleteSoft(View view)
		{
			view.Deleted = true;

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> DeleteHard(View view)
		{
			DbContext.Views.Remove(view);

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> DeleteHardViewState(ViewState viewState)
		{
			DbContext.ViewStates.Remove(viewState);

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> DeleteViewField(int moduleId, string fieldName)
		{
			var view = await DbContext.ViewFields
				.Where(x => x.ViewId == moduleId && x.Field == fieldName)
				.FirstAsync();

			DbContext.ViewFields.Remove(view);
			await DbContext.SaveChangesAsync();
			return view.Id;
		}

		public async Task<ViewState> GetViewState(int moduleId, int userId)
		{
			var viewState = await DbContext.ViewStates
				.FirstOrDefaultAsync(x => x.ModuleId == moduleId && x.UserId == userId && !x.Deleted);

			return viewState;
		}

		public async Task<int> CreateViewState(ViewState viewState)
		{
			DbContext.ViewStates.Add(viewState);

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> UpdateViewState(ViewState viewState)
		{
			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> DeleteViewShare(ViewShares view, TenantUser user)
		{
			user.SharedViews.Remove(view);

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> Count()
		{
			var count = DbContext.Modules
			   .Where(x => !x.Deleted).Count();
			return count;
		}

		public async Task<ICollection<View>> Find(PaginationModel paginationModel)
		{
			var views = GetPaginationGQuery(paginationModel)
				.Skip(paginationModel.Offset * paginationModel.Limit)
				.Take(paginationModel.Limit).ToList();

			if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
			{
				var propertyInfo = typeof(View).GetProperty(paginationModel.OrderColumn);

				if (paginationModel.OrderType == "asc")
				{
					views = views.OrderBy(x => propertyInfo.GetValue(x, null)).ToList();
				}
				else
				{
					views = views.OrderByDescending(x => propertyInfo.GetValue(x, null)).ToList();
				}

			}

			return views;

		}

		private IQueryable<View> GetPaginationGQuery(PaginationModel paginationModel, bool withIncludes = true)
		{
			return DbContext.Views
				 .Where(x => !x.Deleted);

		}
	}
}
