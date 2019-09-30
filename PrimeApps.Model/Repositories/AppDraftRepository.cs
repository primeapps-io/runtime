using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PrimeApps.Model.Context;
using PrimeApps.Model.Enums;
using PrimeApps.Model.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PrimeApps.Model.Entities.Studio;
using PrimeApps.Model.Common;

namespace PrimeApps.Model.Repositories
{
    public class AppDraftRepository : RepositoryBaseStudio, IAppDraftRepository
    {
        public AppDraftRepository(StudioDBContext dbContext, IConfiguration configuration)
            : base(dbContext, configuration)
        {
        }

        public List<int> GetAppIdsByOrganizationId(int organizationId)
        {
            return DbContext.Apps
                .Where(x => x.OrganizationId == organizationId && !x.Deleted)
                .Select(x => x.Id)
                .ToList();
        }

        public async Task<AppDraft> Get(string name)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .Where(x => x.Name == name && !x.Deleted)
                .FirstOrDefaultAsync();
        }

        public async Task<AppDraft> Get(int id)
        {
            return await DbContext.Apps
                .Include(x => x.Setting)
                .Where(x => x.Id == id && !x.Deleted)
                //.Include(x => x.Organization)
                .FirstOrDefaultAsync();
        }

        public async Task<List<AppDraft>> GetUserApps(int userId, int organizationId, string search = "", int page = 0)
        {
            var teamIds = await DbContext.TeamUsers.Where(x => x.UserId == userId && !x.Team.Deleted).Select(x => x.TeamId).ToListAsync();

            var appCollabrator = await DbContext.AppCollaborators
                .Where(x => !x.Deleted && x.AppDraft.OrganizationId == organizationId && (x.UserId == userId || (x.Team != null && teamIds.Contains((int)x.TeamId))) && (string.IsNullOrEmpty(search) || x.AppDraft.Label.Contains(search)))
                .Select(x => new AppDraft
                {
                    Id = x.AppDraft.Id,
                    OrganizationId = x.AppDraft.OrganizationId,
                    Name = x.AppDraft.Name,
                    Label = x.AppDraft.Label,
                    Description = x.AppDraft.Description,
                    Logo = x.AppDraft.Logo,
                    TempletId = x.AppDraft.TempletId,
                    Icon = x.AppDraft.Icon,
                    Color = x.AppDraft.Color
                })
                .Skip(50 * page)
                .Take(50)
                .Distinct()
                .ToListAsync();

            return appCollabrator;
        }
        
        

        public async Task<AppDraft> GetWithPackages(int id)
        {
            return await DbContext.Apps
                .Include(x => x.Packages)
                .Where(x => x.Id == id && !x.Deleted)
                //.Include(x => x.Organization)
                .FirstOrDefaultAsync();
        }
        
        public async Task<int> Create(AppDraft app)
        {
            DbContext.Apps.Add(app);
            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Update(AppDraft app)
        {
            return await DbContext.SaveChangesAsync();
        }

        public async Task<AppDraftSetting> GetAuthTheme(int id)
        {
            return await DbContext.AppSettings
                .Where(x => x.AppId == id).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateAuthTheme(int id, JObject model)
        {
            var appSettings = DbContext.AppSettings.FirstOrDefault(x => x.AppId == id);
            var jsonData = JsonConvert.SerializeObject(model);
            appSettings.AuthTheme = jsonData;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<AppDraftSetting> GetAppTheme(int id)
        {
            return await DbContext.AppSettings
                .Where(x => x.AppId == id).FirstOrDefaultAsync();
        }

        public async Task<int> UpdateAppTheme(int id, JObject model)
        {
            var appSettings = DbContext.AppSettings.FirstOrDefault(x => x.AppId == id);
            var jsonData = JsonConvert.SerializeObject(model);
            appSettings.AppTheme = jsonData;

            return await DbContext.SaveChangesAsync();
        }

        public async Task<int> Delete(AppDraft app)
        {
            app.Deleted = true;
            return await DbContext.SaveChangesAsync();
        }

        public async Task<List<int>> GetByTeamId(int id)
        {
            return await DbContext.AppCollaborators
                .Where(x => x.TeamId == id && !x.Deleted)
                .Select(x => x.AppId)
                .ToListAsync();
        }

        public async Task<List<AppDraft>> GetAllByUserId(int userId, string search = "", int page = 0)
        {
            var teamIds = await DbContext.TeamUsers.Where(x => x.UserId == userId && !x.Team.Deleted).Select(x => x.TeamId).ToListAsync();

            var appCollabrator = await DbContext.AppCollaborators
                .Where(x => !x.Deleted && (x.UserId == userId || (x.Team != null && teamIds.Contains((int)x.TeamId))) && (string.IsNullOrEmpty(search) || x.AppDraft.Label.ToLower().Contains(search.ToLower())))
                .Select(x => x.AppDraft)
                .Skip(500 * page)
                .Take(500)
                .Distinct()
                .ToListAsync();

            return appCollabrator;
        }

        public async Task<List<AppCollaborator>> GetAppCollaborators(int appId)
        {
            return await DbContext.AppCollaborators
                .Where(x => x.AppId == appId && !x.Deleted)
                .ToListAsync();
        }

		public async Task<int> CreateAppTemplate(AppDraftTemplate template)
		{
			DbContext.AppTemplates.Add(template);

			return await DbContext.SaveChangesAsync();
		}

		public async Task<int> UpdateAppTemplate(AppDraftTemplate template)
		{
			return await DbContext.SaveChangesAsync();
		}
		public async Task<AppDraft> AppGetByName(string appName)
		{
			var app = DbContext.Apps.FirstOrDefaultAsync(x => !x.Deleted && x.Name == appName);

			return await app;
		}
		public async Task<ICollection<AppDraftTemplate>> Find(PaginationModel paginationModel, int? appId)
		{
			var templates = DbContext.AppTemplates
				.Where(x => !x.Deleted && x.Type == AppTemplateType.Email && x.AppId == appId)
				.OrderByDescending(x => x.Id) //&& x.Active
				.Skip(paginationModel.Offset * paginationModel.Limit)
				.Take(paginationModel.Limit);

			if (paginationModel.OrderColumn != null && paginationModel.OrderType != null)
			{
				var propertyInfo = typeof(AppDraftTemplate).GetProperty(char.ToUpper(paginationModel.OrderColumn[0]) + paginationModel.OrderColumn.Substring(1));

				if (paginationModel.OrderType == "asc")
				{
					templates = templates.OrderBy(x => propertyInfo.GetValue(x, null));
				}
				else
				{
					templates = templates.OrderByDescending(x => propertyInfo.GetValue(x, null));
				}

			}

			return await templates.ToListAsync();
		}

		public int Count(int appId)
		{
			var count = DbContext.AppTemplates
			   .Where(x => !x.Deleted && x.Type == AppTemplateType.Email && x.AppId == appId).Count();

			return count;
		}

		public async Task<AppDraftTemplate> GetAppTemplateById(int id)
		{
			var template = await DbContext.AppTemplates.FirstOrDefaultAsync(x => x.Id == id);

			return template;
		}
	}
}